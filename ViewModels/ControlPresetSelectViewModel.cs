using Prism.Commands;
using Prism.Regions;
using QRO.Enumerations;
using QRO.Models;
using QRO.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace QRO.ViewModels
{
    public class ControlPresetSelectViewModel : PresetSelectViewModelBase, INavigationAware
    {
        #region Fields

        private readonly IStorageService _storageService;

        #endregion

        #region Constructors

        public ControlPresetSelectViewModel(IStorageService storageService)
        {
            _storageService = storageService;

            SaveCommand = new DelegateCommand(SaveCurrentAsPreset);
            LoadCommand = new DelegateCommand(LoadSelectedPreset);
            DeleteCommand = new DelegateCommand(DeleteSelectedPreset);
        }

        #endregion

        #region Properties

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand DeleteCommand { get; }

        #endregion

        #region INavigationAware

        /// <summary>
        /// Called by Prism when the user navigates to this view. Refreshes the preset list and
        /// prompts a backup if the current controls.cfg does not match any saved preset.
        /// </summary>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RefreshPresets();
            Application.Current.Dispatcher.InvokeAsync(PromptBackupIfNeeded);
        }

        /// <summary>
        /// Returns <see langword="true"/> so Prism reuses the existing ViewModel instance across navigations.
        /// </summary>
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        /// <summary>
        /// Called by Prism when the user navigates away from this view.
        /// </summary>
        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates <see cref="NewPresetName"/>, then copies the current controls.cfg to the QRO appdata
        /// preset directory. Prompts the user before overwriting an existing preset with the same name.
        /// </summary>
        private void SaveCurrentAsPreset()
        {
            if (string.IsNullOrWhiteSpace(NewPresetName))
            {
                StatusMessage = "Please enter a name for the preset.";
                return;
            }

            if (NewPresetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                StatusMessage = "Preset name contains invalid characters.";
                return;
            }

            if (!_storageService.GameFileExists(FilePresetType.Controls))
            {
                StatusMessage = "controls.cfg not found in your iRacing documents folder.";
                return;
            }

            if (_storageService.PresetExists(FilePresetType.Controls, NewPresetName))
            {
                var overwrite = MessageBox.Show(
                    $"A preset named \"{NewPresetName}\" already exists. Overwrite it?",
                    "Overwrite Preset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                _storageService.SavePreset(FilePresetType.Controls, NewPresetName);
                StatusMessage = $"Preset \"{NewPresetName}\" saved.";
                NewPresetName = string.Empty;
                RefreshPresets();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save preset: {ex.Message}";
            }
        }

        /// <summary>
        /// Confirms with the user, then copies the <see cref="SelectedFilePreset"/> back to the iRacing documents folder as controls.cfg.
        /// </summary>
        private void LoadSelectedPreset()
        {
            if (SelectedFilePreset == null)
            {
                StatusMessage = "Please select a preset to load.";
                return;
            }

            var result = MessageBox.Show(
                $"Load \"{SelectedFilePreset.Name}\"? This will overwrite your current controls.cfg.",
                "Load Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _storageService.LoadPreset(FilePresetType.Controls, SelectedFilePreset.Name);
                StatusMessage = $"Preset \"{SelectedFilePreset.Name}\" loaded.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load preset: {ex.Message}";
            }
        }

        /// <summary>
        /// Confirms with the user, then permanently removes the <see cref="SelectedFilePreset"/> file from the QRO appdata directory and refreshes the preset list.
        /// </summary>
        private void DeleteSelectedPreset()
        {
            if (SelectedFilePreset == null)
            {
                StatusMessage = "Please select a preset to delete.";
                return;
            }

            var result = MessageBox.Show(
                $"Delete preset \"{SelectedFilePreset.Name}\"?",
                "Delete Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _storageService.DeletePreset(FilePresetType.Controls, SelectedFilePreset.Name);
                StatusMessage = $"Preset \"{SelectedFilePreset.Name}\" deleted.";
                RefreshPresets();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to delete preset: {ex.Message}";
            }
        }

        /// <summary>
        /// Reloads the <see cref="Presets"/> collection from the QRO appdata directory.
        /// </summary>
        private void RefreshPresets()
        {
            Presets.Clear();
            foreach (var preset in _storageService.GetPresets(FilePresetType.Controls))
                Presets.Add(preset);
        }

        /// <summary>
        /// Checks whether the current controls.cfg matches any saved preset. If not, prompts the user to save a timestamped backup before they make changes.
        /// </summary>
        private void PromptBackupIfNeeded()
        {
            if (!_storageService.GameFileExists(FilePresetType.Controls))
                return;
            if (_storageService.CurrentFileMatchesAnyPreset(FilePresetType.Controls))
                return;

            var result = MessageBox.Show(
                "Your current controls.cfg doesn't match any saved preset. Would you like to back it up?",
                "Back Up Controls",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NewPresetName = "Backup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                SaveCurrentAsPreset();
            }
        }

        #endregion
    }
}

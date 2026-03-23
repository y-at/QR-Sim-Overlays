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
    public class GraphicsPresetSelectViewModel : PresetSelectViewModelBase, INavigationAware
    {
        #region Fields

        private readonly IStorageService _storageService;
        private bool _isVrMode = false;

        private FilePresetType ActiveFilePresetType => _isVrMode ? FilePresetType.GraphicsVR : FilePresetType.GraphicsMonitor;

        #endregion

        #region Constructors

        public GraphicsPresetSelectViewModel(IStorageService storageService)
        {
            _storageService = storageService;

            SaveCommand = new DelegateCommand(SaveCurrentAsPreset);
            LoadCommand = new DelegateCommand(LoadSelectedPreset);
            DeleteCommand = new DelegateCommand(DeleteSelectedPreset);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Boolean indicating whether the user is currently managing VR graphics settings (true) or Monitor graphics settings (false). Changing this value refreshes the preset list and prompts a backup if needed.
        /// </summary>
        public bool IsVrMode
        {
            get => _isVrMode;
            set
            {
                if (SetProperty(ref _isVrMode, value))
                {
                    OnPropertyChanged(nameof(ModeLabel));
                    OnPropertyChanged(nameof(GameFileName));
                    RefreshPresets();
                }
            }
        }

        /// <summary>
        /// Inverse of <see cref="IsVrMode"/> for easier binding to a toggle in the view. True when managing Monitor settings, false when managing VR settings.
        /// </summary>
        public bool IsMonitorMode
        {
            get => !_isVrMode;
            set
            {
                if (SetProperty(ref _isVrMode, !value))
                {
                    OnPropertyChanged(nameof(ModeLabel));
                    OnPropertyChanged(nameof(GameFileName));
                    RefreshPresets();
                }
            }
        }

        /// <summary>
        /// Gets the display label indicating the current mode.
        /// </summary>
        public string ModeLabel => _isVrMode ? "VR" : "Monitor";

        /// <summary>
        /// Gets the name of the game configuration file based on the current rendering mode.
        /// </summary>
        public string GameFileName => _isVrMode ? "rendererDX11OpenXR.ini" : "RendererDX11Monitor.ini";

        /// <summary>
        /// Gets the command that saves the current data or state.
        /// </summary>
        public DelegateCommand SaveCommand { get; }

        /// <summary>
        /// Gets the command that initiates the loading operation.
        /// </summary>
        public DelegateCommand LoadCommand { get; }

        /// <summary>
        /// Gets the command that deletes the selected item or entity.
        /// </summary>
        public DelegateCommand DeleteCommand { get; }

        #endregion

        #region Public Methods

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RefreshPresets();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates <see cref="NewPresetName"/>, then copies the current graphics settings file to the QRO appdata preset directory for the active mode (Monitor or VR). Prompts the user before overwriting an existing preset.
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

            if (!_storageService.GameFileExists(ActiveFilePresetType))
            {
                StatusMessage = $"{GameFileName} not found in your iRacing documents folder.";
                return;
            }

            if (_storageService.PresetExists(ActiveFilePresetType, NewPresetName))
            {
                var overwrite = MessageBox.Show($"A preset named \"{NewPresetName}\" already exists. Overwrite it?", "Overwrite Preset", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                _storageService.SavePreset(ActiveFilePresetType, NewPresetName);
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
        /// Confirms with the user, then copies the <see cref="SelectedFilePreset"/> back to the iRacing documents folder under the game's required filename for the active mode.
        /// </summary>
        private void LoadSelectedPreset()
        {
            if (SelectedFilePreset == null)
            {
                StatusMessage = "Please select a preset to load.";
                return;
            }

            var result = MessageBox.Show($"Load \"{SelectedFilePreset.Name}\"? This will overwrite your current {GameFileName}.", "Load Preset", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _storageService.LoadPreset(ActiveFilePresetType, SelectedFilePreset.Name);
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

            var result = MessageBox.Show($"Delete preset \"{SelectedFilePreset.Name}\"?", "Delete Preset", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _storageService.DeletePreset(ActiveFilePresetType, SelectedFilePreset.Name);
                StatusMessage = $"Preset \"{SelectedFilePreset.Name}\" deleted.";
                RefreshPresets();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to delete preset: {ex.Message}";
            }
        }

        /// <summary>
        /// Reloads the <see cref="Presets"/> collection from the QRO appdata directory for the currently active mode.
        /// </summary>
        private void RefreshPresets()
        {
            Presets.Clear();
            foreach (var preset in _storageService.GetPresets(ActiveFilePresetType))
                Presets.Add(preset);
        }

        /// <summary>
        /// Checks whether the current graphics settings file matches any saved preset for the active mode. If not, prompts the user to save a timestamped backup before they make changes.
        /// </summary>
        private void PromptBackupIfNeeded()
        {
            if (!_storageService.GameFileExists(ActiveFilePresetType))
                return;
            if (_storageService.CurrentFileMatchesAnyPreset(ActiveFilePresetType))
                return;

            var result = MessageBox.Show($"Your current {GameFileName} doesn't match any saved preset. Would you like to back it up?", "Back Up Graphics Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NewPresetName = "Backup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                SaveCurrentAsPreset();
            }
        }

        #endregion
    }
}

using Prism.Commands;
using Prism.Regions;
using QRO.Enumerations;
using QRO.Models;
using QRO.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace QRO.ViewModels
{
    public class GraphicsPresetSelectViewModel : PresetSelectViewModelBase, INavigationAware
    {
        #region Fields

        private readonly IStorageService _storageService;
        private bool _isVrMode = false;
        private readonly ObservableCollection<string> _vrRenderModeCollection = new ObservableCollection<string>();
        private string _selectedVrRenderMode;
        private string _loadedPresetStatus;

        private FilePresetType ActiveFilePresetType => GetActivePresetType();

        #endregion

        #region Constructors

        public GraphicsPresetSelectViewModel(IStorageService storageService)
        {
            _storageService = storageService;

            PopulateVrRenderModes();
            _selectedVrRenderMode = VrRenderModeCollection.FirstOrDefault();

            SaveCommand = new DelegateCommand(SaveCurrentAsPreset);
            LoadCommand = new DelegateCommand(LoadSelectedPreset);
            DeleteCommand = new DelegateCommand(DeleteSelectedPreset);
            RenameCommand = new DelegateCommand(RenameSelectedPreset);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Collection of available VR render modes.
        /// </summary>
        public ObservableCollection<string> VrRenderModeCollection => _vrRenderModeCollection;

        /// <summary>
        /// The selected VR render mode as a string. Changing this value will trigger any necessary updates to the graphics settings file when in VR mode. This property is only relevant when <see cref="IsVrMode"/> is true.
        /// </summary>
        public string SelectedVrRenderMode
        {
            get => _selectedVrRenderMode;
            set
            {
                if (SetProperty(ref _selectedVrRenderMode, value))
                {
                    OnPropertyChanged(nameof(SelectedVrRenderMode));
                    OnPropertyChanged(nameof(GameFileName));
                    RefreshLoadedPresetStatus();
                }
            }
        }

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
                    OnPropertyChanged(nameof(IsVrMode));
                    RefreshPresets();
                    RefreshLoadedPresetStatus();
                }
            }
        }

        /// <summary>
        /// Inverse of <see cref="IsVrMode"/> for easier binding to a toggle in the view. True when managing Monitor settings, false when managing VR settings.
        /// </summary>
        public bool IsMonitorMode
        {
            get => !_isVrMode;
        }

        /// <summary>
        /// Gets the display label indicating the current mode.
        /// </summary>
        public string ModeLabel => _isVrMode ? "VR" : "Monitor";

        /// <summary>
        /// Gets the name of the game configuration file based on the current rendering mode.
        /// </summary>
        public string GameFileName => _isVrMode ? $"RendererDX11{SelectedVrRenderMode}.ini" : "RendererDX11Monitor.ini";

        /// <summary>
        /// Describes which saved preset (if any) matches the graphics settings file currently loaded for the active mode.
        /// </summary>
        public string LoadedPresetStatus
        {
            get => _loadedPresetStatus;
            private set => SetProperty(ref _loadedPresetStatus, value);
        }

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

        /// <summary>
        /// Gets the command that renames the selected preset.
        /// </summary>
        public DelegateCommand RenameCommand { get; }

        #endregion

        #region Public Methods

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RefreshPresets();
            RefreshLoadedPresetStatus();
            Application.Current.Dispatcher.InvokeAsync(PromptBackupIfNeeded);
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
                RefreshLoadedPresetStatus();
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
                RefreshLoadedPresetStatus();
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
                RefreshLoadedPresetStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to delete preset: {ex.Message}";
            }
        }

        /// <summary>
        /// Validates the new name and renames the <see cref="SelectedFilePreset"/> to the name entered in <see cref="PresetSelectViewModelBase.NewPresetName"/>.
        /// </summary>
        private void RenameSelectedPreset()
        {
            if (SelectedFilePreset == null)
            {
                StatusMessage = "Please select a preset to rename.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPresetName))
            {
                StatusMessage = "Enter the new name in the box above, then click Rename Selected.";
                return;
            }

            if (NewPresetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                StatusMessage = "Preset name contains invalid characters.";
                return;
            }

            if (string.Equals(NewPresetName, SelectedFilePreset.Name, StringComparison.Ordinal))
            {
                StatusMessage = "That's already the current name.";
                return;
            }

            if (_storageService.PresetExists(ActiveFilePresetType, NewPresetName))
            {
                StatusMessage = $"A preset named \"{NewPresetName}\" already exists.";
                return;
            }

            try
            {
                var newName = NewPresetName;
                _storageService.RenamePreset(ActiveFilePresetType, SelectedFilePreset.Name, newName);
                StatusMessage = $"Renamed to \"{newName}\".";
                NewPresetName = string.Empty;
                RefreshPresets();
                RefreshLoadedPresetStatus();
                SelectedFilePreset = Presets.FirstOrDefault(preset => preset.Name == newName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to rename preset: {ex.Message}";
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
        /// Scans the saved presets for the active mode for one whose contents match the current graphics settings file and updates <see cref="LoadedPresetStatus"/> accordingly.
        /// </summary>
        private void RefreshLoadedPresetStatus()
        {
            if (!_storageService.GameFileExists(ActiveFilePresetType))
            {
                LoadedPresetStatus = $"No {GameFileName} found in your iRacing documents folder.";
                return;
            }

            var matchingPreset = _storageService.GetMatchingPreset(ActiveFilePresetType);
            LoadedPresetStatus = matchingPreset != null
                ? $"Currently loaded profile: {matchingPreset.Name}"
                : $"Currently loaded {GameFileName} doesn't match any saved preset.";
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

        private void PopulateVrRenderModes()
        {
            _vrRenderModeCollection.Clear();
            foreach (var mode in Enum.GetValues(typeof(VrRenderMode)))
            {
                _vrRenderModeCollection.Add(mode.ToString());
            }
        }

        private FilePresetType GetActivePresetType()
        {
            if (!_isVrMode)
                return FilePresetType.GraphicsMonitor;

            return SelectedVrRenderMode switch
                {
                "OpenXR" => FilePresetType.GraphicsVrOpenXr,
                "OpenVR" => FilePresetType.GraphicsVrOpenVr,
                "Oculus" => FilePresetType.GraphicsVrOculus,
                _ => throw new InvalidOperationException($"Unexpected VR render mode: {SelectedVrRenderMode}")
            };
        }

        #endregion
    }
}

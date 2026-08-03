using QRO.ViewModels.Overlays;
using QRO.Views;
using QRO.Views.Overlays;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QRO.ViewModels;
using Prism.Commands;
using Prism.Regions;
using QRO.Enumerations;
using QRO.Models;
using QRO.Services;
using QRO.Services.Factories;

namespace QRO.ViewModels
{
    public class OverlaySelectorViewModel : ViewModelBase
    {
        #region Fields

        private static ITelemetryServiceManager _telemetryServiceManager;
        private static IOverlayWindowManagerService _overlayWindowManagerService;
        private readonly IOverlayProfileService _overlayProfileService;

        private bool _isConnectedToSimulator;
        private ObservableCollection<FilePresetModel> _profiles = new ObservableCollection<FilePresetModel>();
        private FilePresetModel _selectedProfile;
        private FilePresetModel _autoLoadProfile;
        private string _newProfileName;
        private string _statusMessage;
        private bool _autoLoadEnabled;
        private bool _lockOverlaysEnabled;
        private bool _unloadOnDisconnectEnabled;

        /// <summary>
        /// Boolean indicating whether the telemetry service is currently connected to the simulator.
        /// </summary>
        public bool IsConnectedToSimulator
        {
            get => _isConnectedToSimulator;
            set => SetProperty(ref _isConnectedToSimulator, value);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Collection of saved overlay layout profiles, loaded from the QRO appdata directory.
        /// </summary>
        public ObservableCollection<FilePresetModel> Profiles
        {
            get => _profiles;
            private set => SetProperty(ref _profiles, value);
        }

        /// <summary>
        /// The currently selected profile from the <see cref="Profiles"/> collection. Used as the target for Load, Rename and Delete operations.
        /// </summary>
        public FilePresetModel SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        /// <summary>
        /// The profile to automatically apply when the simulator connects, if <see cref="AutoLoadEnabled"/> is true.
        /// </summary>
        public FilePresetModel AutoLoadProfile
        {
            get => _autoLoadProfile;
            set
            {
                if (SetProperty(ref _autoLoadProfile, value))
                    _overlayProfileService.SetAutoLoadProfileName(value?.Name);
            }
        }

        /// <summary>
        /// Whether a profile should be automatically applied when the simulator connects.
        /// </summary>
        public bool AutoLoadEnabled
        {
            get => _autoLoadEnabled;
            set
            {
                if (SetProperty(ref _autoLoadEnabled, value))
                    _overlayProfileService.SetAutoLoadEnabled(value);
            }
        }

        /// <summary>
        /// Whether overlays are locked: click-through to the sim underneath, and un-grabbable for dragging or resizing.
        /// </summary>
        public bool LockOverlaysEnabled
        {
            get => _lockOverlaysEnabled;
            set
            {
                if (SetProperty(ref _lockOverlaysEnabled, value))
                {
                    _overlayWindowManagerService.SetOverlaysLocked(value);
                    _overlayProfileService.SetLockOverlaysEnabled(value);
                }
            }
        }

        /// <summary>
        /// Whether open overlays should automatically close when the simulator disconnects.
        /// </summary>
        public bool UnloadOnDisconnectEnabled
        {
            get => _unloadOnDisconnectEnabled;
            set
            {
                if (SetProperty(ref _unloadOnDisconnectEnabled, value))
                    _overlayProfileService.SetUnloadOnDisconnectEnabled(value);
            }
        }

        /// <summary>
        /// The name entered by the user for saving the current overlay layout as a new profile.
        /// </summary>
        public string NewProfileName
        {
            get => _newProfileName;
            set => SetProperty(ref _newProfileName, value);
        }

        /// <summary>
        /// The message presented to the user to provide feedback on profile operations or validation errors.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public DelegateCommand SaveProfileCommand { get; }
        public DelegateCommand LoadProfileCommand { get; }
        public DelegateCommand RenameProfileCommand { get; }
        public DelegateCommand DeleteProfileCommand { get; }

        #endregion

        #region Constructors

        public OverlaySelectorViewModel(ITelemetryServiceManager telemetryServiceManager, IOverlayWindowManagerService overlayWindowManagerService, IOverlayProfileService overlayProfileService)
        {
            // Set default to disconnected (red)
            _isConnectedToSimulator = false;

            _telemetryServiceManager = telemetryServiceManager;
            _overlayWindowManagerService = overlayWindowManagerService;
            _overlayProfileService = overlayProfileService;

            // Will check once multiple simulators are supported, but for now we know iRacing is the only one.
            //_telemetryServiceManager.CheckForActiveSimulators();
            _telemetryServiceManager.SetActiveService(Simulator.iRacing);

            _telemetryServiceManager.Connected += OnTelemetryConnected;
            _telemetryServiceManager.Disconnected += OnTelemetryDisconnected;

            SaveProfileCommand = new DelegateCommand(SaveCurrentAsProfile);
            LoadProfileCommand = new DelegateCommand(LoadSelectedProfile);
            RenameProfileCommand = new DelegateCommand(RenameSelectedProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteSelectedProfile);

            RefreshProfiles();
            _autoLoadEnabled = _overlayProfileService.GetAutoLoadEnabled();
            _autoLoadProfile = Profiles.FirstOrDefault(p => p.Name == _overlayProfileService.GetAutoLoadProfileName());

            _unloadOnDisconnectEnabled = _overlayProfileService.GetUnloadOnDisconnectEnabled();
            _lockOverlaysEnabled = _overlayProfileService.GetLockOverlaysEnabled();
            _overlayWindowManagerService.SetOverlaysLocked(_lockOverlaysEnabled);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the input traces overlay view.
        /// </summary>
        public void LoadInputTracesView()
        {
            OverlayName overlay = OverlayName.InputTraces;
            if (_overlayWindowManagerService.IsOverlayOpen(overlay))
                _overlayWindowManagerService.CloseOverlay(overlay);
            else
                _overlayWindowManagerService.SpawnOverlay(overlay);
        }

        /// <summary>
        /// Loads the attitude indicator overlay view.
        /// </summary>
        public void LoadAttitudeView()
        {
            OverlayName overlay = OverlayName.AttitudeIndicator;
            if (_overlayWindowManagerService.IsOverlayOpen(overlay))
                _overlayWindowManagerService.CloseOverlay(overlay);
            else
                _overlayWindowManagerService.SpawnOverlay(overlay);
        }

        /// <summary>
        /// Loads the wind direction compass overlay view.
        /// </summary>
        public void LoadWindDirectionCompass()
        {
            OverlayName overlay = OverlayName.WindDirectionCompass;
            if (_overlayWindowManagerService.IsOverlayOpen(overlay))
                _overlayWindowManagerService.CloseOverlay(overlay);
            else
                _overlayWindowManagerService.SpawnOverlay(overlay);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates <see cref="NewProfileName"/>, then saves the current open/closed state, position and size of every overlay as a new profile. Prompts the user before overwriting an existing profile with the same name.
        /// </summary>
        private void SaveCurrentAsProfile()
        {
            if (string.IsNullOrWhiteSpace(NewProfileName))
            {
                StatusMessage = "Please enter a name for the profile.";
                return;
            }

            if (NewProfileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                StatusMessage = "Profile name contains invalid characters.";
                return;
            }

            if (_overlayProfileService.ProfileExists(NewProfileName))
            {
                var overwrite = MessageBox.Show(
                    $"A profile named \"{NewProfileName}\" already exists. Overwrite it?",
                    "Overwrite Profile",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                var layouts = _overlayWindowManagerService.GetCurrentLayouts();
                _overlayProfileService.SaveProfile(NewProfileName, layouts);
                StatusMessage = $"Profile \"{NewProfileName}\" saved.";
                NewProfileName = string.Empty;
                RefreshProfiles();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save profile: {ex.Message}";
            }
        }

        /// <summary>
        /// Applies the <see cref="SelectedProfile"/>: opens, closes, repositions and resizes overlays to match the saved layout.
        /// </summary>
        private void LoadSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                StatusMessage = "Please select a profile to load.";
                return;
            }

            try
            {
                ApplyProfile(SelectedProfile.Name);
                StatusMessage = $"Profile \"{SelectedProfile.Name}\" loaded.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load profile: {ex.Message}";
            }
        }

        /// <summary>
        /// Validates the new name and renames the <see cref="SelectedProfile"/> to the name entered in <see cref="NewProfileName"/>.
        /// </summary>
        private void RenameSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                StatusMessage = "Please select a profile to rename.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewProfileName))
            {
                StatusMessage = "Enter the new name in the box above, then click Rename Selected.";
                return;
            }

            if (NewProfileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                StatusMessage = "Profile name contains invalid characters.";
                return;
            }

            if (string.Equals(NewProfileName, SelectedProfile.Name, StringComparison.Ordinal))
            {
                StatusMessage = "That's already the current name.";
                return;
            }

            if (_overlayProfileService.ProfileExists(NewProfileName))
            {
                StatusMessage = $"A profile named \"{NewProfileName}\" already exists.";
                return;
            }

            try
            {
                var oldName = SelectedProfile.Name;
                var newName = NewProfileName;
                var wasAutoLoadProfile = AutoLoadProfile != null && AutoLoadProfile.Name == oldName;

                _overlayProfileService.RenameProfile(oldName, newName);
                StatusMessage = $"Renamed to \"{newName}\".";
                NewProfileName = string.Empty;
                RefreshProfiles();
                SelectedProfile = Profiles.FirstOrDefault(p => p.Name == newName);

                if (wasAutoLoadProfile)
                    AutoLoadProfile = SelectedProfile;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to rename profile: {ex.Message}";
            }
        }

        /// <summary>
        /// Confirms with the user, then permanently removes the <see cref="SelectedProfile"/> and refreshes the profile list.
        /// </summary>
        private void DeleteSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                StatusMessage = "Please select a profile to delete.";
                return;
            }

            var result = MessageBox.Show(
                $"Delete profile \"{SelectedProfile.Name}\"?",
                "Delete Profile",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var deletedName = SelectedProfile.Name;
                _overlayProfileService.DeleteProfile(deletedName);
                StatusMessage = $"Profile \"{deletedName}\" deleted.";
                RefreshProfiles();

                if (AutoLoadProfile != null && AutoLoadProfile.Name == deletedName)
                    AutoLoadProfile = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to delete profile: {ex.Message}";
            }
        }

        /// <summary>
        /// Reloads the <see cref="Profiles"/> collection from the QRO appdata directory.
        /// </summary>
        private void RefreshProfiles()
        {
            Profiles.Clear();
            foreach (var profile in _overlayProfileService.GetProfiles())
                Profiles.Add(profile);
        }

        /// <summary>
        /// Loads the named profile's saved layouts and applies each one to the live overlay windows.
        /// </summary>
        private void ApplyProfile(string name)
        {
            foreach (var layout in _overlayProfileService.LoadProfile(name))
                _overlayWindowManagerService.ApplyLayout(layout);
        }

        private void OnTelemetryConnected(object sender, EventArgs e)
        {
            // Telemetry events fire from a background SDK thread, but applying a profile creates/moves WPF windows, which must happen on the UI thread.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => OnTelemetryConnected(sender, e));
                return;
            }

            IsConnectedToSimulator = true;

            if (AutoLoadEnabled && AutoLoadProfile != null)
                ApplyProfile(AutoLoadProfile.Name);
        }

        private void OnTelemetryDisconnected(object sender, EventArgs e)
        {
            // Telemetry events fire from a background SDK thread, but closing overlays touches WPF windows, which must happen on the UI thread.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => OnTelemetryDisconnected(sender, e));
                return;
            }

            IsConnectedToSimulator = false;

            if (UnloadOnDisconnectEnabled)
            {
                foreach (var overlay in _overlayWindowManagerService.CurrentOpenOverlays)
                    _overlayWindowManagerService.CloseOverlay(overlay);
            }
        }

        #endregion
    }
}

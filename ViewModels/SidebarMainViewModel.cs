using QRO.Services;
using QRO.ViewModels;
using Prism.Regions;
using QRO.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QRO.ViewModels
{
    public class SidebarMainViewModel : ViewModelBase
    {
        #region Fields

        private readonly ITelemetryServiceManager _telemetryServiceManager;
        private IRegionManager _regionManager;

        private bool _isConnected;
        private string _selectedButton;

        #endregion

        #region Constructors

        SidebarMainViewModel(ITelemetryServiceManager TelemetryServiceManager, IRegionManager regionManager)
        {
            _telemetryServiceManager = TelemetryServiceManager;
            _regionManager = regionManager;

            _telemetryServiceManager.Connected += OnTelemetryConnected;
            _telemetryServiceManager.Disconnected += OnTelemetryDisconnected;

            IsConnected = _telemetryServiceManager.IsConnected;
            SelectedButton = "Overlays"; // Set default selected button
        }

        #endregion

        #region Properties

        /// <summary>
        /// Boolean indicating whether the telemetry service is currently connected to the simulator. Used to determine the color of the connection status indicator in the UI (green if connected, red if not).
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        /// <summary>
        /// Indicates which navigation button is currently selected.
        /// </summary>
        public string SelectedButton
        {
            get => _selectedButton;
            set => SetProperty(ref _selectedButton, value);
        }

        #endregion

        #region Public Methods

        public void OnOverlayTabSelected()
        {
            SelectedButton = "Overlays";
            _regionManager.RequestNavigate(UIRegions.ContentRegion, nameof(OverlaySelectorView));
        }

        public void OnControlsPresetsTabSelected()
        {
            SelectedButton = "ControlsPresets";
            _regionManager.RequestNavigate(UIRegions.ContentRegion, nameof(ControlPresetSelectView));
        }

        public void OnGraphicsPresetsTabSelected()
        {
            SelectedButton = "GraphicsPresets";
            _regionManager.RequestNavigate(UIRegions.ContentRegion, nameof(GraphicsPresetSelectView));
        }

        public void OnRaceEngineerTabSelected()
        {
            SelectedButton = "RaceEngineer";
            _regionManager.RequestNavigate(UIRegions.ContentRegion, nameof(RaceEngineerChatView));
        }

        public void OnGithubButtonPress()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/y-at/QR-Sim-Overlays",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open GitHub link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Private Methods
        private void OnTelemetryConnected(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        private void OnTelemetryDisconnected(object sender, EventArgs e)
        {
            IsConnected = false;
        }

        #endregion
    }
}

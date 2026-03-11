using iRacing_Quick_Release.Enumerations;
using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels.Overlays;
using iRacing_Quick_Release.Views;
using iRacing_Quick_Release.Views.Overlays;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using iRacing_Quick_Release.ViewModels;
using Prism.Regions;
using QRO.Enumerations;
using QRO.Services;
using QRO.Services.Factories;

namespace QRO.ViewModels
{
    public class OverlaySelectorViewModel : ViewModelBase
    {
        #region Fields

        private static ITelemetryServiceManager _telemetryServiceManager;
        private static IOverlayWindowManagerService _overlayWindowManagerService;
        private bool _isConnectedToSimulator;

        /// <summary>
        /// Boolean indicating whether the telemetry service is currently connected to the simulator.
        /// </summary>
        public bool IsConnectedToSimulator
        {
            get => _isConnectedToSimulator;
            set
            {
                if (_isConnectedToSimulator != value)
                {
                    _isConnectedToSimulator = value;
                }
            }
        }
        #endregion

        #region Properties


        #endregion

        #region Constructors

        public OverlaySelectorViewModel(ITelemetryServiceManager telemetryServiceManager, IOverlayWindowManagerService overlayWindowManagerService)
        {
            // Set default to disconnected (red)
            _isConnectedToSimulator = false;

            _telemetryServiceManager = telemetryServiceManager;
            _overlayWindowManagerService = overlayWindowManagerService;

            // Will check once multiple simulators are supported, but for now we know iRacing is the only one.
            //_telemetryServiceManager.CheckForActiveSimulators();
            _telemetryServiceManager.SetActiveService(Simulator.iRacing);

            _telemetryServiceManager.Connected += OnTelemetryConnected;
            _telemetryServiceManager.Disconnected += OnTelemetryDisconnected;

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

        private void OnTelemetryConnected(object sender, EventArgs e)
        {
            IsConnectedToSimulator = true;
        }

        private void OnTelemetryDisconnected(object sender, EventArgs e)
        {
            IsConnectedToSimulator = false;
        }

        #endregion
    }
}

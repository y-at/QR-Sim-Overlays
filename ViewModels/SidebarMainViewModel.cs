using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels;

namespace QRO.ViewModels
{
    public class SidebarMainViewModel : ViewModelBase
    {
        #region Fields

        private readonly ITelemetryServiceManager _telemetryServiceManager;

        private bool _isConnected;

        #endregion

        #region Constructors

        SidebarMainViewModel(ITelemetryServiceManager TelemetryServiceManager)
        {
            _telemetryServiceManager = TelemetryServiceManager;

            _telemetryServiceManager.Connected += OnTelemetryConnected;
            _telemetryServiceManager.Disconnected += OnTelemetryDisconnected;

            IsConnected = _telemetryServiceManager.IsConnected;
        }

        #endregion

        #region Properties

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        #endregion

        #region Public Methods

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

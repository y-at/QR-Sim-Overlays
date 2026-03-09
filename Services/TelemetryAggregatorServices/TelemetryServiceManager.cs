using iRacing_Quick_Release.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacing_Quick_Release.Enumerations;
using IRSDKSharper;

namespace iRacing_Quick_Release.Services
{
    public class TelemetryServiceManager : ITelemetryServiceManager
    {
        #region Fields

        private ITelemetryService _activeService;
        private readonly Dictionary<Simulator, ITelemetryService> _services;

        #endregion

        #region Properties

        /// <summary>
        /// Occurs when new data is received for the session
        /// </summary>
        public event Action<SessionModel> DataReceived;

        /// <summary>
        /// Occurs when the telemetry service connects to the simulator.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when the telemetry service disconnects from the simulator.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Is the telemetry service connected to the simulator.
        /// </summary>
        public bool IsConnected => _activeService?.IsConnected ?? false;

        /// <summary>
        /// Is the telemetry service receiving data from simulator.
        /// </summary>
        public bool IsDataTransmitting => _activeService?.IsDataTransmitting ?? false;

        #endregion

        #region Constructors

        public TelemetryServiceManager()
        {
            //Only iRacing for now.
            _services = new Dictionary<Simulator, ITelemetryService>
            {
                { Simulator.iRacing, new iRacingTelemetryService(new IRacingSdk()) }
            };

            // Temp solution, will fix soon
            InitializeServices(); 
            SetActiveService(Simulator.iRacing);
        }

        #endregion

        #region Public Methods

        public void InitializeServices()
        {
            foreach (var service in _services.Values)
            {
                service.Start();
            }
        }

        /// <summary>
        /// Sets what telemetry service to use to read data from various simulators.
        /// </summary>
        /// <param name="simulatorService">The telemetry service relevant to the game loaded</param>
        public void SetActiveService(Simulator simulatorService)
        {
            // Activate requested service
            if (_services.TryGetValue(simulatorService, out var newService))
            {
                _activeService = newService;
                _activeService.DataReceived += OnDataReceived;
                _activeService.Connected += OnConnected;
                _activeService.Disconnected += OnDisconnected;
            }
            else
            {
                throw new ArgumentException("Invalid service name", nameof(simulatorService));
            }
        }

        /// <summary>
        /// Start the selected telemetry service for data reception.
        /// </summary>
        public void Start()
        {
            _activeService?.Start();
        }

        /// <summary>
        /// End the selected telemetry service for data reception.
        /// </summary>
        public void Stop()
        {
            _activeService?.Stop();
        }

        #endregion

        #region Private Methods

        private void OnDataReceived(SessionModel session)
        {
            DataReceived?.Invoke(session);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Connected?.Invoke(sender, e);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Disconnected?.Invoke(sender, e);
        }

        #endregion
    }
}

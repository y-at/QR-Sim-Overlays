using iRacing_Quick_Release.Models;
using iRacing_Quick_Release.Enumerations;
using iRacing_Quick_Release.Services;
using IRSDKSharper;
using System;
using System.Collections.Generic;
using System.Linq;
using QRO.Services;

namespace iRacing_Quick_Release.Services
{
    public class TelemetryServiceManager : ITelemetryServiceManager, IDisposable
    {
        #region Fields

        private ITelemetryService _activeService;
        private readonly Dictionary<Simulator, ITelemetryService> _services;
        private readonly ISimulatorProcessMonitor _processMonitor;
        private bool _isRunning;
        private bool _autoStartEnabled;

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

        /// <summary>
        /// Is the telemetry service currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Enable automatic start/stop based on simulator process detection.
        /// </summary>
        public bool AutoStartEnabled
        {
            get => _autoStartEnabled;
            set
            {
                _autoStartEnabled = value;
                if (value)
                    EnableAutoStart();
                else
                    DisableAutoStart();
            }
        }

        #endregion

        #region Constructors

        public TelemetryServiceManager(ISimulatorProcessMonitor processMonitor)
        {
            _processMonitor = processMonitor ?? throw new ArgumentNullException(nameof(processMonitor));

            _services = new Dictionary<Simulator, ITelemetryService>
            {
                { Simulator.iRacing, new iRacingTelemetryService(new IRacingSdk()) }
            };

            SetActiveService(Simulator.iRacing);
            _isRunning = false;
        }

        #endregion

        #region Public Methods

        public void InitializeServices()
        {
            // Services are now initialized lazily
        }

        /// <summary>
        /// Sets what telemetry service to use to read data from various simulators.
        /// </summary>
        /// <param name="simulatorService">The telemetry service relevant to the game loaded</param>
        public void SetActiveService(Simulator simulatorService)
        {
            // Stop current service before switching
            if (_isRunning && _activeService != null)
            {
                _activeService.Stop();
                _activeService.DataReceived -= OnDataReceived;
                _activeService.Connected -= OnConnected;
                _activeService.Disconnected -= OnDisconnected;
            }

            // Activate requested service
            if (_services.TryGetValue(simulatorService, out var newService))
            {
                _activeService = newService;
                _activeService.DataReceived += OnDataReceived;
                _activeService.Connected += OnConnected;
                _activeService.Disconnected += OnDisconnected;

                // If currently running, start the new service
                if (_isRunning)
                {
                    _activeService.Start();
                }
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
            if (!_isRunning && _activeService != null)
            {
                _activeService.Start();
                _isRunning = true;
            }
        }

        /// <summary>
        /// Stop the selected telemetry service for data reception.
        /// </summary>
        public void Stop()
        {
            if (_isRunning && _activeService != null)
            {
                _activeService.Stop();
                _isRunning = false;
            }
        }

        #endregion

        #region Private Methods

        private void EnableAutoStart()
        {
            _processMonitor.ProcessStateChanged += OnSimulatorProcessStateChanged;
            _processMonitor.StartMonitoring();
        }

        private void DisableAutoStart()
        {
            _processMonitor.ProcessStateChanged -= OnSimulatorProcessStateChanged;
            _processMonitor.StopMonitoring();
        }

        private void OnSimulatorProcessStateChanged(Simulator simulator, bool isRunning)
        {
            if (isRunning && !_isRunning)
            {
                System.Diagnostics.Trace.WriteLine($"{nameof(simulator)} detected, starting telemetry...");
                Start();
            }
            else if (!isRunning && _isRunning)
            {
                System.Diagnostics.Trace.WriteLine($"{nameof(simulator)} closed, stopping telemetry...");
                Stop();
            }
        }

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

        public void Dispose()
        {
            Stop();
            DisableAutoStart();
            (_activeService as IDisposable)?.Dispose();
        }

        #endregion
    }
}

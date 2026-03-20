using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRSDKSharper;
using QRO.Enumerations;
using QRO.Models;

namespace QRO.Services
{
    public interface ITelemetryServiceManager
    {
        /// <summary>
        /// Occurs when new data is received for the session
        /// </summary>
        event Action<SessionModel> DataReceived;
        /// <summary>
        /// Occurs when the telemetry service connects to the simulator.
        /// </summary>
        event EventHandler Connected;
        /// <summary>
        /// Occurs when the telemetry service disconnects from the simulator.
        /// </summary>
        event EventHandler Disconnected;
        /// <summary>
        /// Is the telemetry service connected to the simulator.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Is the telemetry service receiving data from simulator.
        /// </summary>
        bool IsDataTransmitting { get; }
        /// <summary>
        /// Gets or sets a value indicating whether automatic startup is enabled.
        /// </summary>
        bool AutoStartEnabled { get; set; }
        /// <summary>
        /// Initializes and configures all required application services.
        /// </summary>
        void InitializeServices();
        /// <summary>
        /// Sets what telemetry service to use to read data from various simulators.
        /// </summary>
        /// <param name="simulatorService">The telemetry service relevant to the game loaded</param>
        void SetActiveService(Simulator simulatorService);
        /// <summary>
        /// Start the selected telemetry service for data reception.
        /// </summary>
        void Start();
        /// <summary>
        /// End the selected telemetry service for data reception.
        /// </summary>
        void Stop();

        void Dispose();
    }
}

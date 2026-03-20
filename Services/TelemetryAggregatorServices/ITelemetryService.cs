using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRO.Models;

namespace QRO.Services
{
    public interface ITelemetryService
    {
        /// <summary>
        /// Is the telemetry service connected to the simulator.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Is the telemetry service receiving data from simulator.
        /// </summary>
        bool IsDataTransmitting { get; }
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
        /// Start telemetry data reception.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop telemetry data reception.
        /// </summary>
        void Stop();
    }
}

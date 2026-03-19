using iRacing_Quick_Release.Enumerations;
using System;
using System.Collections.Generic;

namespace QRO.Services
{
    public interface ISimulatorProcessMonitor
    {
        /// <summary>
        /// Occurs when a simulator's process state changes (running/stopped).
        /// </summary>
        event Action<Simulator, bool> ProcessStateChanged;

        /// <summary>
        /// Gets whether any simulator process is currently running.
        /// </summary>
        bool IsProcessRunning { get; }

        /// <summary>
        /// Gets the current state of all simulators.
        /// </summary>
        IReadOnlyDictionary<Simulator, bool> SimulatorStates { get; }

        /// <summary>
        /// Starts monitoring simulator processes.
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stops monitoring simulator processes.
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// Checks if a specific simulator is currently running.
        /// </summary>
        /// <param name="simulator">The simulator to check</param>
        /// <returns>True if the simulator process is running; otherwise, false</returns>
        bool IsSimulatorRunning(Simulator simulator);
    }
}

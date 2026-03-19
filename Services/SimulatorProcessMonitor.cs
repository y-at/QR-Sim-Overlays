using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRacing_Quick_Release.Enumerations;

namespace QRO.Services
{
    public class SimulatorProcessMonitor : ISimulatorProcessMonitor, IDisposable
    {
        #region Fields

        private CancellationTokenSource _cancellationTokenSource;
        private Task _monitoringTask;
        private readonly Dictionary<Simulator, bool> _simulatorStates = new();
        private const int MonitoringIntervalSeconds = 5;

        #endregion

        #region Constructors

        public SimulatorProcessMonitor()
        {
            InitializeSimulatorStates();
        }

        #endregion

        #region Properties

        public event Action<Simulator, bool> ProcessStateChanged;

        public bool IsProcessRunning => _simulatorStates.Values.Any(state => state);

        public IReadOnlyDictionary<Simulator, bool> SimulatorStates => _simulatorStates;

        #endregion

        #region Public Methods

        public void StartMonitoring()
        {
            if (_monitoringTask != null && !_monitoringTask.IsCompleted)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _monitoringTask = MonitorProcessAsync(_cancellationTokenSource.Token);
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            _monitoringTask?.Wait(TimeSpan.FromSeconds(2));
        }

        public bool IsSimulatorRunning(Simulator simulator)
        {
            return _simulatorStates.TryGetValue(simulator, out var isRunning) && isRunning;
        }

        public void Dispose()
        {
            StopMonitoring();
            _cancellationTokenSource?.Dispose();
        }

        #endregion

        #region Private Methods

        private void InitializeSimulatorStates()
        {
            foreach (var simulator in Enum.GetValues(typeof(Simulator)).Cast<Simulator>())
            {
                _simulatorStates[simulator] = false;
            }
        }

        private async Task MonitorProcessAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var simulator in _simulatorStates.Keys.ToList())
                    {
                        var isRunning = IsProcessRunning_Internal(simulator);

                        if (isRunning != _simulatorStates[simulator])
                        {
                            _simulatorStates[simulator] = isRunning;
                            ProcessStateChanged?.Invoke(simulator, isRunning);
                            
                            System.Diagnostics.Trace.WriteLine(
                                $"Simulator '{simulator}' is now {(isRunning ? "running" : "stopped")}");
                        }
                    }

                    await Task.Delay(MonitoringIntervalSeconds * 1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Error in SimulatorProcessMonitor: {ex.Message}");
                }
            }
        }

        private bool IsProcessRunning_Internal(Simulator simulator)
        {
            try
            {
                return Process.GetProcessesByName(GetApplicationNameFromEnum(simulator)).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private string GetApplicationNameFromEnum(Simulator simulator)
        {
            switch (simulator)
            {
                case Simulator.iRacing:
                    return "iRacingSim64DX11";
                default:
                    throw new ArgumentOutOfRangeException(nameof(simulator), $"Unsupported simulator: {simulator}");
            }
        }

        #endregion
    }
}

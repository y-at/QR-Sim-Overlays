using IRSDKSharper;
using System;
using QRO.Models;

namespace QRO.Services
{
    public class iRacingTelemetryService : ITelemetryService
    {
        #region Fields

        private readonly IRacingSdk _iracingSdk;
        private bool _isConnected;
        private bool _isDataTransmitting;

        #endregion

        #region Properties

        /// <summary>
        /// Is the telemetry service connected to the simulator.
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Is the telemetry service receiving data from simulator.
        /// </summary>
        public bool IsDataTransmitting => _isDataTransmitting;

        /// <summary>
        /// Occurs when new data is received for the session.
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

        #endregion

        #region Constructor

        public iRacingTelemetryService(IRacingSdk iRacingSdk)
        {
            _iracingSdk = iRacingSdk;

            _iracingSdk.OnException += OnException;
            _iracingSdk.OnConnected += OnConnected;
            _iracingSdk.OnDisconnected += OnDisconnected;
            _iracingSdk.OnSessionInfo += OnSessionInfo;
            _iracingSdk.OnTelemetryData += OnTelemetryData;
            _iracingSdk.OnStopped += OnStopped;
            _iracingSdk.OnDebugLog += OnDebugLog;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start telemetry data reception.
        /// </summary>
        public void Start() => _iracingSdk.Start();

        /// <summary>
        /// Stop telemetry data reception.
        /// </summary>
        public void Stop() => _iracingSdk.Stop();

        #endregion

        #region Private Methods

        private SessionModel BuildSessionModel()
        {
            var data = _iracingSdk.Data;
            var weekendInfo = data.SessionInfo.WeekendInfo;

            return new SessionModel
            {
                TrackName = weekendInfo.TrackName,
                SessionTime = data.GetFloat("SessionTime", 0),
                TrackLength = float.Parse(weekendInfo.TrackLengthOfficial.Replace(" km", "")),
                AirPressure = float.Parse(weekendInfo.TrackAirPressure.Remove(5)),
                AirTemp = float.Parse(weekendInfo.TrackAirTemp.Replace(" C", "")),
                TrackTemp = data.GetFloat("TrackTemp", 0),
                WindSpeed = data.GetFloat("WindVel", 0) * 3.6f, // m/s -> km/h
                WindDirection = data.GetFloat("WindDir", 0) * 180 / (float)Math.PI,
                IsPaused = data.GetInt("ReplayPlaySpeed", 0) == 0,
            };
        }

        private CarDataModel BuildCarData(int index, bool isPlayerCar)
        {
            var data = _iracingSdk.Data;
            var driver = data.SessionInfo.DriverInfo.Drivers[index];

            var carData = new CarDataModel
            {
                IsPlayerCar = isPlayerCar,
                Position = data.GetInt("CarIdxPosition", index),
                PositionInClass = data.GetInt("CarIdxClassPosition", index),
                ClassType = data.GetInt("CarIdxClass", index),
                LapsStarted = data.GetInt("CarIdxLap", index),
                LapsCompleted = data.GetInt("CarIdxLapCompleted", index),
                CurrentLapDistancePercentage = data.GetFloat("CarIdxLapDistPct", index),
            };

            carData.Drivers.Name = driver.TeamName;
            carData.Drivers.IRating = driver.IRating;
            carData.Drivers.SafetyRating = driver.LicString;
            carData.Drivers.IsAI = Convert.ToBoolean(driver.CarIsAI);

            if (isPlayerCar)
            {
                carData.Throttle = data.GetFloat("Throttle", 0);
                carData.Brake = data.GetFloat("Brake", 0);
                carData.Clutch = (data.GetFloat("Clutch", 0) - 1) * -1; //  1=released, invert to 0=released
                carData.Speed = data.GetFloat("Speed", 0) * 3.6f;       // m/s → km/h
                carData.Gear = data.GetInt("Gear", 0);
                carData.BrakeBias = data.GetFloat("dcBrakeBias", 0);
                carData.TractionControl = data.GetInt("dcTractionControl", 0);
                carData.Rpm = data.GetFloat("RPM", 0);
                carData.Redline = data.GetFloat("PlayerCarSLBlinkRPM", 0);
                carData.SteeringAngle = data.GetFloat("SteeringWheelAngle", 0) * 180 / (float)Math.PI;
                carData.SteeringAngleMax = data.GetFloat("SteeringWheelAngleMax", 0) * 180 / (float)Math.PI;
                carData.IsABSActive = data.GetBool("BrakeABSactive", 0);
                carData.RollFromLevel = data.GetFloat("Roll", 0) * 180 / (float)Math.PI;
                carData.PitchFromLevel = data.GetFloat("Pitch", 0) * 180 / (float)Math.PI;
                carData.YawFromNorth = (data.GetFloat("Yaw", 0) * -180 / (float)Math.PI) + 180;
            }

            return carData;
        }

        #endregion

        #region Event Handlers

        private void OnException(Exception exception)
        {
            System.Diagnostics.Trace.WriteLine($"OnException() {nameof(IRSDKSharper)} exception triggered in {nameof(iRacingTelemetryService)}.");
        }

        private void OnConnected()
        {
            System.Diagnostics.Trace.WriteLine($"OnConnected() fired in {nameof(iRacingTelemetryService)}.");
            _isConnected = true;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected()
        {
            System.Diagnostics.Trace.WriteLine($"OnDisconnected() fired in {nameof(iRacingTelemetryService)}.");
            _isDataTransmitting = false;
            _isConnected = false;
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnSessionInfo()
        {
            _isDataTransmitting = true;
            System.Diagnostics.Trace.WriteLine($"OnSessionInfo fired in {nameof(iRacingTelemetryService)}.");
        }

        private void OnTelemetryData()
        {
            if (!_isDataTransmitting) return;

            var sessionData = BuildSessionModel();
            int playerCarIdx = _iracingSdk.Data.GetInt("PlayerCarIdx", 0);

            for (int i = 0; i < _iracingSdk.Data.SessionInfo.DriverInfo.Drivers.Count; i++)
            {
                sessionData.Cars.Add(BuildCarData(i, i == playerCarIdx));
            }

            DataReceived?.Invoke(sessionData);
        }

        private void OnStopped()
        {
            System.Diagnostics.Trace.WriteLine($"OnStopped() fired in {nameof(iRacingTelemetryService)}.");
        }

        private void OnDebugLog(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        #endregion
    }
}

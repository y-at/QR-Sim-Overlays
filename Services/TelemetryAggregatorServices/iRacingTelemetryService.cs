using iRacing_Quick_Release.Models;
using IRSDKSharper;
using System;
using System.ComponentModel;


namespace iRacing_Quick_Release.Services
{
    public class iRacingTelemetryService : ITelemetryService
    {
        #region Properties
        private IRacingSdk _iracingSdk;
        private bool _isConnected;
        private bool _isDataTransmitting;
        #endregion

        #region Fields

        /// <summary>
        /// Is the telemetry service connected to the simulator.
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
        }

        /// <summary>
        /// Is the telemetry service receiving data from simulator.
        /// </summary>
        public bool IsDataTransmitting
        {
            get => _isDataTransmitting;
        }

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

        #endregion

        #region Constructors

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
        public void Start()
        {
            _iracingSdk.Start();
        }

        /// <summary>
        /// Stop telemetry data reception.
        /// </summary>
        public void Stop()
        {
            _iracingSdk.Stop();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Event Listeners

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
            // If session info available, start processing data
            if (!_isDataTransmitting || _iracingSdk == null) return;

            //Debug.WriteLine($"OnTelemetryData fired!");

            var sessionData = new SessionModel
            {
                TrackName = _iracingSdk.Data.SessionInfo.WeekendInfo.TrackName,
                SessionTime = _iracingSdk.Data.GetFloat("SessionTime", 0),
                TrackLength = float.Parse(_iracingSdk.Data.SessionInfo.WeekendInfo.TrackLengthOfficial.Replace(" km", "")), // convert m to km
                AirPressure = float.Parse(_iracingSdk.Data.SessionInfo.WeekendInfo.TrackAirPressure.Remove(5)),
                AirTemp = float.Parse(_iracingSdk.Data.SessionInfo.WeekendInfo.TrackAirTemp.Replace(" C", "")),
                TrackTemp = _iracingSdk.Data.GetFloat("TrackTemp", 0),
                WindSpeed = _iracingSdk.Data.GetFloat("WindVel", 0) * 3.6f, //Convert m/s to km/h
                WindDirection = _iracingSdk.Data.GetFloat("WindDir", 0) * 180 / (float)Math.PI,
                //SkyState = _iracingSdk.Data.SessionInfo.WeekendInfo.TrackSkies,
                IsPaused = _iracingSdk.Data.GetInt("ReplayPlaySpeed", 0) == 0,
            };

            for (int i = 0; i < _iracingSdk.Data.SessionInfo.DriverInfo.Drivers.Count; i++)
            {
                var carData = new CarDataModel();
                    // Get player specific data
                    if (i == _iracingSdk.Data.GetInt("PlayerCarIdx", 0))
                    {
                        carData.IsPlayerCar = true;
                        carData.Throttle = _iracingSdk.Data.GetFloat("Throttle", 0);
                        carData.Brake = _iracingSdk.Data.GetFloat("Brake", 0);
                        carData.Clutch = (_iracingSdk.Data.GetFloat("Clutch", 0) - 1) * -1;
                        carData.Speed = _iracingSdk.Data.GetFloat("Speed", 0) * 3.6f;
                        carData.Gear = _iracingSdk.Data.GetInt("Gear", 0);
                        carData.BrakeBias = _iracingSdk.Data.GetFloat("dcBrakeBias", 0);
                        carData.TractionControl = _iracingSdk.Data.GetInt("dcTractionControl", 0);
                        //carData.ABSLevel = _iracingSdk.Data.GetInt("dcABS", 0);
                        carData.Rpm = _iracingSdk.Data.GetFloat("RPM", 0);
                        carData.Redline = _iracingSdk.Data.GetFloat("PlayerCarSLBlinkRPM", 0);
                        carData.SteeringAngle = _iracingSdk.Data.GetFloat("SteeringWheelAngle", 0) * 180 / (float)Math.PI;
                        carData.SteeringAngleMax = _iracingSdk.Data.GetFloat("SteeringWheelAngleMax", 0) * 180 / (float)Math.PI;
                        carData.IsABSActive = _iracingSdk.Data.GetBool("BrakeABSactive", 0);
                        carData.RollFromLevel = _iracingSdk.Data.GetFloat("Roll", 0) * 180 / (float)Math.PI;
                        carData.PitchFromLevel = _iracingSdk.Data.GetFloat("Pitch", 0) * 180 / (float)Math.PI;
                        carData.YawFromNorth = (_iracingSdk.Data.GetFloat("Yaw", 0) * -180 / (float)Math.PI) + 180;
                }
                // Get data for everyone
                carData.Drivers.Name = _iracingSdk.Data.SessionInfo.DriverInfo.Drivers[i].TeamName;
                carData.Position = _iracingSdk.Data.GetInt("CarIdxPosition", i);
                carData.PositionInClass = _iracingSdk.Data.GetInt("CarIdxClassPosition", i);
                carData.ClassType = _iracingSdk.Data.GetInt("CarIdxClass", i);
                carData.LapsStarted = _iracingSdk.Data.GetInt("CarIdxLap", i);
                carData.LapsCompleted = _iracingSdk.Data.GetInt("CarIdxLapCompleted", i);
                carData.Drivers.IRating = _iracingSdk.Data.SessionInfo.DriverInfo.Drivers[i].IRating;
                carData.Drivers.SafetyRating = _iracingSdk.Data.SessionInfo.DriverInfo.Drivers[i].LicString;
                carData.Drivers.IsAI = Convert.ToBoolean(_iracingSdk.Data.SessionInfo.DriverInfo.Drivers[i].CarIsAI);
                carData.CurrentLapDistancePercentage = _iracingSdk.Data.GetFloat("CarIdxLapDistPct", i);


                sessionData.Cars.Add(carData);
            }
            //System.Diagnostics.Trace.WriteLine(_iracingSdk.Data.GetInt("TrackWetness", 0));

            var test = _iracingSdk.Data;
            DataReceived?.Invoke(sessionData);
        }

        private void OnStopped()
        {
            _iracingSdk.Stop();
            System.Diagnostics.Trace.WriteLine($"OnStopped() fired in {nameof(iRacingTelemetryService)}.");
        }

        private void OnDebugLog(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        #endregion

    }
}


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacing_Quick_Release.Models
{
    public class CarDataModel
    {
        #region Fields

        private bool _isPlayerCar;
        private DriverDataModel _drivers = new DriverDataModel();

        private int _classType;
        private int _carNumber;
        private int _position;
        private int _positionInClass;
        private bool _hasFastRepair;
        private int _incidents;

        private int _lapsStarted; // Lap num represented in sim GUI
        private int _lapsCompleted;
        private int _bestLapTimeNumber;
        private float _currentLapDistanceMeters;
        private float _currentLapDistancePercent;
        private string _trackSurface;

        private bool _isCarInPit;
        private bool _isCarOnTrack;
        private bool _isCarInGarage;

        private float _steeringAngle;
        private float _steeringAngleMax;
        private float _throttle;
        private float _brake;
        private float _clutch;
        private bool _isABSActive;
        private int _gear;
        private float _brakeBias;
        private int _tractionControl;
        private int _ABSLevel;
        private float _rpm;
        private float _redline;
        private float _speed;
        private float _fuelLevel;

        private float _yawFromNorth;
        private float _pitchFromLevel;
        private float _rollFromLevel;

        #endregion

        #region Properties
        /// <summary>
        /// Is current car the player's car.
        /// </summary>
        public bool IsPlayerCar
        {
            get { return _isPlayerCar; }
            set { _isPlayerCar = value; }
        }
        /// <summary>
        /// Current driver of the cars data.
        /// </summary>
        public DriverDataModel Drivers
        {
            get { return _drivers; }
            set { _drivers = value; }
        }
        /// <summary>
        /// Number of the car.
        /// </summary>
        public int CarNumber
        {
            get { return _carNumber; }
            set { _carNumber = value; }
        }
        /// <summary>
        /// Position of the car
        /// </summary>
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        /// <summary>
        /// Position of car in class.
        /// </summary>
        public int PositionInClass
        {
            get { return _positionInClass; }
            set { _positionInClass = value; }
        }
        /// <summary>
        /// Count of incidents incurred by the car.
        /// </summary>
        public int Incidents
        {
            get { return _incidents; }
            set { _incidents = value; }
        }
        /// <summary>
        /// Number of laps started by the car.
        /// </summary>
        public int LapsStarted
        {
            get { return _lapsStarted; }
            set { _lapsStarted = value; }
        }
        /// <summary>
        /// Number of laps completed by the car.
        /// </summary>
        public int LapsCompleted
        {
            get { return _lapsCompleted; }
            set { _lapsCompleted = value; }
        }
        /// <summary>
        /// What class type the car is for multiclass racing.
        /// </summary>
        public int ClassType
        {
            get { return _classType; }
            set { _classType = value; }
        }

        public bool IsCarInPit
        {
            get { return _isCarInPit; }
            set { _isCarInPit = value; }
        }
        /// <summary>
        /// Is car on track or in the pits.
        /// </summary>
        public bool IsCarOnTrack
        {
            get { return _isCarOnTrack; }
            set { _isCarOnTrack = value; }
        }
        /// <summary>
        /// Is the car in the garage (not in pits or on track).
        /// </summary>
        public bool IsCarInGarage
        {
            get { return _isCarInGarage; }
            set { _isCarInGarage = value; }
        }
        /// <summary>
        /// Player's steering wheel angle in degrees.
        /// Left negative right positive.
        /// </summary>
        public float SteeringAngle
        {
            get => _steeringAngle;
            set { if (_isPlayerCar) _steeringAngle = value; }
        }
        /// <summary>
        /// Player's steering wheel angle in degrees at maximum lock.
        /// left negative right positive.
        /// </summary>
        public float SteeringAngleMax
        {
            get => _steeringAngleMax;
            set { if (_isPlayerCar) _steeringAngleMax = value; }
        }
        /// <summary>
        /// Position of throttle pedal. (0.0 to 1.0)
        /// </summary>
        public float Throttle
        {
            get => _throttle;
            set { if (_isPlayerCar) _throttle = value; }
        }
        /// <summary>
        /// Position of brake pedal. (0.0 to 1.0)
        /// </summary>
        public float Brake
        {
            get => _brake;
            set { if (_isPlayerCar) _brake = value; }
        }
        /// <summary>
        /// Position of clutch pedal. (0.0 to 1.0)
        /// </summary>
        public float Clutch
        {
            get => _clutch;
            set { if (_isPlayerCar) _clutch = value; }
        }
        /// <summary>
        /// Is the ABS system currently active.
        /// </summary>
        public bool IsABSActive
        {
            get => _isABSActive;
            set { if (_isPlayerCar) _isABSActive = value; }
        }
        /// <summary>
        /// What gear the car is currently in.
        /// </summary>
        public int Gear
        {
            get => _gear;
            set { if (_isPlayerCar) _gear = value; }
        }
        /// <summary>
        /// Brake bias setting of the car. (Each car can represent this differently)
        /// </summary>
        public float BrakeBias
        {
            get => _brakeBias;
            set
            { if (_isPlayerCar) _brakeBias = value; }
        }
        /// <summary>
        /// Traction control setting of the car. (Each car can represent this differently)
        /// </summary>
        public int TractionControl
        {
            get => _tractionControl;
            set { if (_isPlayerCar) _tractionControl = value; }
        }
        /// <summary>
        /// abs level setting of the car. (Each car can represent this differently)
        /// </summary>
        public int ABSLevel
        {
            get => _ABSLevel;
            set { if (_isPlayerCar) _ABSLevel = value; }
        }
        /// <summary>
        /// Revolutions per minute of the car's engine.
        /// </summary>
        public float Rpm
        {
            get => _rpm;
            set { if (_isPlayerCar) _rpm = value; }
        }
        /// <summary>
        /// What the drivers car redline is.
        /// </summary>
        public float Redline
        {
            get => _redline;
            set { if (_isPlayerCar) _redline = value; }
        }
        /// <summary>
        /// What is the cars current speed in km/h.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set { if (_isPlayerCar) _speed = value; }
        }
        /// <summary>
        /// Fuel level of the car in liters.
        /// </summary>
        public float FuelLevel
        {
            get => _fuelLevel;
            set { if (_isPlayerCar) _fuelLevel = value; }
        }
        /// <summary>
        /// Degrees from north in a clockwise direction.
        /// </summary>
        public float YawFromNorth
        {
            get => _yawFromNorth;
            set { if (_isPlayerCar) _yawFromNorth = value; }
        }
        /// <summary>
        /// Current pitch from level in degrees.
        /// </summary>
        public float PitchFromLevel
        {
            get => _pitchFromLevel;
            set { if (_isPlayerCar) _pitchFromLevel = value; }
        }
        /// <summary>
        /// Current roll from level in degrees.
        /// </summary>
        public float RollFromLevel
        {
            get => _rollFromLevel;
            set { if (_isPlayerCar) _rollFromLevel = value; }
        }
        /// <summary>
        /// Current lap distance in meters
        /// </summary>
        public float CurrentLapDistanceMeters
        {
            get => _currentLapDistanceMeters;
            set { _currentLapDistanceMeters = value; }
        }
        /// <summary>
        /// Current lap distance as a percentage of the lap
        /// </summary>
        public float CurrentLapDistancePercentage
        {
            get => _currentLapDistancePercent;
            set { _currentLapDistancePercent = value; }
        }
        /// <summary>
        /// What surface the car is currently on.
        /// </summary>
        public string CarOnTrackSurface
        {
            get => _trackSurface;
            set { _trackSurface = value; }
        }

        #endregion
    }
}

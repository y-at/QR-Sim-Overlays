using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacing_Quick_Release.Models
{
    public class SessionModel
    {
        #region Fields

        private bool _isPaused;
        private float _sessionTime;

        private string _trackName; // completed in irTS
        private float _trackLength; // Track length in km
        private float _trackTemp; // completed in irTS
        private float _airTemp; // completed in irTS
        private int _trackWetness;
        private bool _weatherDeclaredWet;
        private float _airPressure; // completed in irTS

        private int _skyState; // 0 = clear, 1 = scattered clouds, 2 = broken clouds, 3 = overcast

        private float _windSpeed; // completed in irTS
        private float _windDirection; // completed in irTS

        private Collection<CarDataModel> _cars;

        #endregion

        #region Properties

        /// <summary>
        /// Is game state paused (live and replay mode).
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        /// <summary>
        /// Seconds past since session started (in iRacing)
        /// </summary>
        public float SessionTime
        {
            get => _sessionTime;
            set => _sessionTime = value;
        }

        /// <summary>
        /// Track name
        /// </summary>
        public string TrackName
        {
            get => _trackName;
            set => _trackName = value;
        }

        /// <summary>
        /// Track length in meters
        /// </summary>
        public float TrackLength
        {
            get => _trackLength;
            set
            {
                if (value >= 0)
                {
                    _trackLength = value;
                }
            }
        }

        /// <summary>
        /// Track temperature. (Celsius)
        /// </summary>
        public float TrackTemp
        {
            get => _trackTemp;
            set => _trackTemp = value;
        }

        /// <summary>
        /// Air temperature. (Celsius)
        /// </summary>
        public float AirTemp
        {
            get => _airTemp;
            set => _airTemp = value;
        }

        /// <summary>
        /// Track wetness (as percentage?).
        /// </summary>
        public int TrackWetness
        {
            get => _trackWetness;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _trackWetness = value;
                }
            }
        }

        /// <summary>
        /// Session has been declared wet.
        /// </summary>
        public bool WeatherDeclaredWet
        {
            get => _weatherDeclaredWet;
            set => _weatherDeclaredWet = value;
        }

        /// <summary>
        /// Air pressure. (kpa?)
        /// </summary>
        public float AirPressure
        {
            get => _airPressure;
            set
            {
                if (value >= 0)
                {
                    _airPressure = value;
                }
            }
        }

        /// <summary>
        /// Sky state: 0 = clear, 1 = scattered clouds, 2 = broken clouds, 3 = overcast.
        /// </summary>
        public int SkyState
        {
            get => _skyState;
            set
            {
                if (value >= 0 && value <= 3)
                {
                    _skyState = 0;
                }
            }
        }

        /// <summary>
        /// Wind speed. (km/h)
        /// </summary>
        public float WindSpeed
        {
            get => _windSpeed;
            set
            {
                if (value >= 0)
                {
                    _windSpeed = value;
                }
            }
        }

        /// <summary>
        /// Wind direction in degrees.
        /// </summary>
        public float WindDirection
        {
            get => _windDirection;
            set
            {
                if (value >= 0 && value < 360)
                {
                    _windDirection = value;
                }
            }
        }

        /// <summary>
        /// Collection of cars in the session. Never null.
        /// </summary>
        public Collection<CarDataModel> Cars
        {
            get => _cars;
            set => _cars = value ?? new Collection<CarDataModel>();
        }

        #endregion

        #region Constructors
        public SessionModel()
        {
            _cars = new Collection<CarDataModel>();
        }

        #endregion
    }
}

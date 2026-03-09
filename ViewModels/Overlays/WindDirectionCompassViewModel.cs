using iRacing_Quick_Release.Models;
using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace iRacing_Quick_Release.Views.Overlays
{
    public class WindDirectionCompassViewModel : ViewModelBase
    {
        #region Fields

        ITelemetryServiceManager _telemetryServiceManager;

        float _playerCarYawFromNorth;

        float _windSpeed;
        float _windDirection;

        private float _trackTemperature;
        private float _airTemperature;

        #endregion

        #region Properties

        public float PlayerCarYawFromNorth
        {
            get => _playerCarYawFromNorth;
            set
            {
                _playerCarYawFromNorth = value;
                OnPropertyChanged(nameof(PlayerCarYawFromNorth));
            }
        }

        public float WindSpeed
        {
            get => _windSpeed;
            set
            {
                _windSpeed = value;
                OnPropertyChanged(nameof(WindSpeed));
                OnPropertyChanged(nameof(WindSpeedString));
            }
        }

        public string WindSpeedString => $"{WindSpeed:0.0} km/h";

        public float WindDirection
        {
            get => _windDirection;
            set
            {
                _windDirection = value;
                OnPropertyChanged(nameof(WindDirection));
            }
        }

        public string TrackTemperatureString => $"{TrackTemperature:0.0}°C";
        public float TrackTemperature
        {
            get => _trackTemperature;
            set
            {
                _trackTemperature = value;
                OnPropertyChanged(nameof(TrackTemperature));
                OnPropertyChanged(nameof(TrackTemperatureString));
            }
        }

        public string AirTemperatureString => $"{AirTemperature:0.0}°C";
        public float AirTemperature
        {
            get => _airTemperature;
            set
            {
                _airTemperature = value;
                OnPropertyChanged(nameof(AirTemperature));
                OnPropertyChanged(nameof(AirTemperatureString));
            }
        }

        #endregion

        #region Constructor

        public WindDirectionCompassViewModel(ITelemetryServiceManager telemetryServiceManager)
        {
            _telemetryServiceManager = telemetryServiceManager;
            _telemetryServiceManager.DataReceived += OnDataReceived;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        private void OnDataReceived(SessionModel session)
        {
            // Renders on the UI thread
            // If we're not on the UI thread, marshal the call to the UI thread
            // Prevents hickups / freezes
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => OnDataReceived(session));
                return;
            }

            // Stops taking data when paused
            if (session.IsPaused == true)
            {
                return;
            }

            CarDataModel playerCar = session.Cars.FirstOrDefault(car => car.IsPlayerCar);

            PlayerCarYawFromNorth = playerCar.YawFromNorth * -1 + 360; // Rotation of compass is inverted without this correction?
            WindDirection = session.WindDirection + PlayerCarYawFromNorth;
            WindSpeed = session.WindSpeed;

            TrackTemperature = session.TrackTemp;
            AirTemperature = session.AirTemp;
        }

        #endregion
    }
}

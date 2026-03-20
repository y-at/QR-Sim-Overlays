using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QRO.Models;
using QRO.Services;

namespace QRO.ViewModels.Overlays
{
    public class AttitudeViewModel : ViewModelBase
    {
        #region Fields

        private float _roll;
        private float _pitch;

        private string _stringRoll;
        private string _stringPitch;

        ITelemetryServiceManager _telemetryServiceManager;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the roll angle, in degrees.
        /// </summary>
        public float Roll
        {
            get => _roll;
            set
            {
                _roll = value;
                OnPropertyChanged(nameof(Roll));
            }
        }

        /// <summary>
        /// Gets or sets the pitch adjustment value.
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                OnPropertyChanged(nameof(Pitch));
                OnPropertyChanged(nameof(PitchScaled));
            }
        }

        /// <summary>
        /// Gets the pitch value scaled by a factor of two. Used in the AttitudeIndicatorView to adjust the sensitivity of the pitch display.
        /// </summary>
        public float PitchScaled
        {
            get => _pitch * 2;
        }

        /// <summary>
        /// The string value representing the current roll.
        /// </summary>
        public string StringRoll
        {
            get => _stringRoll;
            set
            {
                _stringRoll = value;
                OnPropertyChanged(nameof(StringRoll));
            }
        }

        /// <summary>
        /// The string value representing the current pitch.
        /// </summary>
        public string StringPitch
        {
            get => _stringPitch;
            set
            {
                _stringPitch = value;
                OnPropertyChanged(nameof(StringPitch));
            }
        }

        #endregion

        #region Constructors

        public AttitudeViewModel(ITelemetryServiceManager telemetryServiceManager)
        {
            _telemetryServiceManager = telemetryServiceManager;
            _telemetryServiceManager.DataReceived += OnDataReceived;
        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region Event Listeners

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
            if (session.IsPaused == true || IsDisposed)
            {
                return;
            }
              
            CarDataModel playerCar = session.Cars.FirstOrDefault(car => car.IsPlayerCar);

            Roll = playerCar.RollFromLevel;
            Pitch = playerCar.PitchFromLevel * -1;
            StringRoll = "Roll: " + Math.Round(Roll,2);
            StringPitch = "Pitch: " + Math.Round(Pitch,2);
        }

        #endregion
    }
}

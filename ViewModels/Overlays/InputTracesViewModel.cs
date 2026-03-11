using iRacing_Quick_Release.Models;
using iRacing_Quick_Release.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace iRacing_Quick_Release.ViewModels.Overlays
{
    // Note to self: This class could use a lot of refactoring for efficient data handling and maintainability.
    public class InputTracesViewModel : ViewModelBase
    {
        #region Fields

        const int MaxDataPoints = 480;

        float _throttle;
        float _brake;
        float _clutch;
        float _speed;
        private float _brakeBias;
        private int _absValue;
        private int _tcValue;
        float _minSpeed;
        float _maxSpeed;
        string _gear = "N";
        float _rpm;
        float _redline;
        float _steeringWheelAngle;
        float _steeringWheelAngleMax;
        ITelemetryServiceManager _telemetryServiceManager;

        // Driver control inputs over time plot related
        private AreaSeries _throttleSeries;
        private AreaSeries _brakeSeries;
        private AreaSeries _AbsBrakeSeries;
        private AreaSeries _clutchSeries;
        private LineSeries _steeringSeries;

        private Queue<DataPoint> _throttlePoints = new Queue<DataPoint>();
        private Queue<DataPoint> _brakePoints = new Queue<DataPoint>();
        private Queue<DataPoint> _AbsBrakePoints = new Queue<DataPoint>();
        private Queue<DataPoint> _clutchPoints = new Queue<DataPoint>();
        private Queue<DataPoint> _steeringPoints = new Queue<DataPoint>();

        // Gear fill related
        private double _gearFillPercentage;
        private Brush _gearFill;
        private DispatcherTimer _blinkTimer;
        private bool _isBlinkOn;

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public PlotModel ThrottlePlotModel { get; private set; }

        /// <summary>
        /// Throttle value as a percentage (0-100).
        /// </summary>
        public float Throttle
        {
            get => _throttle;
            set 
            {
                if (value >= 0 && value <= 100)
                {
                    _throttle = value;
                    OnPropertyChanged(nameof(Throttle));
                }
            }
        }

        /// <summary>
        /// Brake value as a percentage (0-100).
        /// </summary>
        public float Brake
        {
            get => _brake;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _brake = value;
                    OnPropertyChanged(nameof(Brake));
                }
            }
        }

        /// <summary>
        /// Clutch value as a percentage (0-100).
        /// </summary>
        public float Clutch
        {
            get => _clutch;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _clutch = value;
                    OnPropertyChanged(nameof(Clutch));
                }
            }
        }

        /// <summary>
        /// Brake bias set by driver
        /// </summary>
        public float BrakeBias
        {
            get => _brakeBias;
            set
            {
                _brakeBias = value;
                OnPropertyChanged(nameof(BrakeBias));
            }
        }

        /// <summary>
        /// Traction control level set by driver
        /// </summary>
        public int TractionControlLevel
        {
            get => _tcValue;
            set
            {
                _tcValue = value;
                OnPropertyChanged(nameof(TractionControlLevel));
            }
        }

        /// <summary>
        /// Antilock brake level set by driver
        /// </summary>
        public int AntilockBrakeLevel
        {
            get => _absValue;
            set
            {
                _absValue = value;
                OnPropertyChanged(nameof(AntilockBrakeLevel));
            }
        }


        /// <summary>
        /// The current speed of the player's car in km/h.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set
            {
                if (value >= 0)
                {
                    _speed = value;
                    OnPropertyChanged(nameof(Speed));
                }
            }
        }

        /// <summary>
        /// The current gear of the player's car.
        /// </summary>
        public string Gear
        {
            get => _gear;
            set
            {
                if (value == "0")
                    _gear = "N";
                if (value == "-1")
                    _gear = "R";
                else 
                    _gear = value;
                OnPropertyChanged(nameof(Gear));
            }
        }

        /// <summary>
        /// The current RPM of the player's car.
        /// </summary>
        public float RPM
        {
            get => _rpm;
            set
            {
                _rpm = value;
                OnPropertyChanged(nameof(RPM));
            }
        }

        /// <summary>
        /// The RPM value at which the player's car will redline.
        /// </summary>
        public float Redline
        {
            get => _redline;
            set
            {
                _redline = value;
                OnPropertyChanged(nameof(Redline));
            }
        }

        /// <summary>
        /// The current steering wheel angle of the player's car.
        /// </summary>
        public float SteeringWheelAngle
        { 
        get => _steeringWheelAngle;
        set
            {
                _steeringWheelAngle = value;
                OnPropertyChanged(nameof(SteeringWheelAngle));
            }
        }

        /// <summary>
        /// The maximum steering wheel angle of the player's car. Used to normalize the steering input for display in the plot.
        /// </summary>
        public float SteeringWheelAngleMax  
        {
            get => _steeringWheelAngleMax;
            set
            {
                _steeringWheelAngleMax = value;
                OnPropertyChanged(nameof(SteeringWheelAngleMax));
            }
        }

        /// <summary>
        /// The current fill percentage of the gear.
        /// </summary>
        public double GearFillPercentage
        {
            get => _gearFillPercentage;
            set
            {
                _gearFillPercentage = value;
                UpdateGearFill();
                OnPropertyChanged(nameof(GearFillPercentage));
            }
        }

        /// <summary>
        /// The colours used to fill the gear display.
        /// </summary>
        public Brush GearFill
        {
            get => _gearFill;
            set
            {
                _gearFill = value;
                OnPropertyChanged(nameof(GearFill));
            }
        }

        #endregion

        #region Constructors

        public InputTracesViewModel(ITelemetryServiceManager telemetryServiceManager)
        {
            _telemetryServiceManager = telemetryServiceManager;
            _telemetryServiceManager.DataReceived += OnDataReceived;

            ThrottlePlotModel = new PlotModel();
            ThrottlePlotModel.Title = "";
            //ThrottlePlotModel.PlotAreaBorderThickness = new OxyThickness(0);
            //ThrottlePlotModel.Background = OxyColors.Transparent;
            //ThrottlePlotModel.PlotAreaBackground = OxyColors.Transparent;
            ThrottlePlotModel.Padding = new OxyThickness(0);
            ThrottlePlotModel.Background.ChangeOpacity(60);
            ThrottlePlotModel.IsLegendVisible = false;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1,
                Maximum = 101,
                IsAxisVisible = false

            };
            ThrottlePlotModel.Axes.Add(xAxis);
            ThrottlePlotModel.Axes.Add(yAxis);
            _clutchSeries = new AreaSeries { Title = "Clutch", LineStyle = LineStyle.Solid, Color = OxyColor.FromAColor(128, OxyColors.Blue) };
            ThrottlePlotModel.Series.Add(_clutchSeries);
            _brakeSeries = new AreaSeries { Title = "Brake", LineStyle = LineStyle.Solid, Color = OxyColor.FromAColor(128, OxyColors.Red) };
            ThrottlePlotModel.Series.Add(_brakeSeries);
            _AbsBrakeSeries = new AreaSeries { Title = "Brake", LineStyle = LineStyle.Solid, Color = OxyColor.FromAColor(128, OxyColors.Orange) };
            ThrottlePlotModel.Series.Add(_AbsBrakeSeries);
            _throttleSeries = new AreaSeries { Title = "Throttle", LineStyle = LineStyle.Solid, Color = OxyColor.FromAColor(128, OxyColors.Green) };
            ThrottlePlotModel.Series.Add(_throttleSeries);
            _steeringSeries = new LineSeries { Title = "Steering", LineStyle = LineStyle.Solid, Color = OxyColors.Gray };
            ThrottlePlotModel.Series.Add(_steeringSeries);

            _blinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) 
            };
            _blinkTimer.Tick += BlinkTimer_Tick;

        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the fill of the gear display based on the current RPM and redline values. Will blink as redline is approached
        /// </summary>
        private void UpdateGearFill()
        {
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0)
            };

            float rpmPercentage = (RPM / Redline) * 100;

            // Blick when above 95%
            if (rpmPercentage >= 95)
            {
                if (!_blinkTimer.IsEnabled)
                    _blinkTimer.Start();

                // Blinking logic
                if (_isBlinkOn)
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                }
                else
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                }
            }
            // Fill normally between 80% and 95%
            else
            {
                if (_blinkTimer.IsEnabled)
                    _blinkTimer.Stop();

                float adjustedFillPercentage;
                if (rpmPercentage < 80)
                {
                    adjustedFillPercentage = 0;
                }
                else
                {
                    adjustedFillPercentage = (rpmPercentage - 80) * 5;
                }

                gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, adjustedFillPercentage / 100.0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.White, adjustedFillPercentage / 100.0));
            }

            GearFill = gradientBrush;
        }

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
                return;

            CarDataModel playerCar = session.Cars.FirstOrDefault(car => car.IsPlayerCar);//.FirstOrDefault(c => c.IsPlayerCar);
            if (playerCar != null)
            {
                //Populate queues to avoid sliding window effect for first data points
                for (int i = _throttlePoints.Count; i < MaxDataPoints; i++)
                {
                    _throttlePoints.Enqueue(new DataPoint(i, 0));
                    _brakePoints.Enqueue(new DataPoint(i, 0));
                    _AbsBrakePoints.Enqueue(new DataPoint(i, 0));
                    _clutchPoints.Enqueue(new DataPoint(i, 0));
                    _steeringPoints.Enqueue(new DataPoint(i, 50));
                }
                // Get current values from player car and update properties
                Throttle = playerCar.Throttle * 100;
                Brake = playerCar.Brake * 100;
                Clutch = playerCar.Clutch * 100;
                Gear = playerCar.Gear.ToString();
                RPM = playerCar.Rpm;
                Redline = playerCar.Redline;
                Speed = (int)playerCar.Speed;
                BrakeBias = playerCar.BrakeBias;
                TractionControlLevel = playerCar.TractionControl;
                SteeringWheelAngle = playerCar.SteeringAngle;
                SteeringWheelAngleMax = playerCar.SteeringAngleMax;
                SteeringWheelAngle = ((playerCar.SteeringAngle / (playerCar.SteeringAngleMax/2)) * 0.5f + 0.5f) * 100;
                GearFillPercentage = RPM;

                // FIFO logic for plotting
                double x = _throttlePoints.Count > 0 ? _throttlePoints.Last().X + 1 : 0;
                var throttlePoint = new DataPoint(x, Throttle);
                var brakePoint = new DataPoint(x, Brake);
                var clutchPoint = new DataPoint(x, Clutch);
                var steeringPoint = new DataPoint(x, SteeringWheelAngle);

                // Remove oldest point if we have reached the maximum number of data points to maintain a sliding window effect
                if (_throttlePoints.Count >= MaxDataPoints)
                {
                    _throttlePoints.Dequeue();
                    _brakePoints.Dequeue();
                    _AbsBrakePoints.Dequeue();
                    _clutchPoints.Dequeue();
                    _steeringPoints.Dequeue();
                }
                    
                // Add new points to the queues
                _throttlePoints.Enqueue(throttlePoint);
                if (!playerCar.IsABSActive) // If ABS is not active, draw red brake area
                {
                    _brakePoints.Enqueue(brakePoint);
                    _AbsBrakePoints.Enqueue(new DataPoint(x, 0));
                }
                else // If ABS is active, draw orange brake area
                {
                    _AbsBrakePoints.Enqueue(brakePoint);
                    _brakePoints.Enqueue(new DataPoint(x, 0));
                }
                _clutchPoints.Enqueue(clutchPoint);
                _steeringPoints.Enqueue(steeringPoint);

                // Remove all points from the series and add the updated points from the queues to update the plot
                _throttleSeries.Points.Clear();
                _brakeSeries.Points.Clear();
                _AbsBrakeSeries.Points.Clear();
                _clutchSeries.Points.Clear();
                _steeringSeries.Points.Clear();

                _throttleSeries.Points.AddRange(_throttlePoints);
                _brakeSeries.Points.AddRange(_brakePoints);
                _AbsBrakeSeries.Points.AddRange(_AbsBrakePoints);
                _clutchSeries.Points.AddRange(_clutchPoints);
                _steeringSeries.Points.AddRange(_steeringPoints);

                ThrottlePlotModel.InvalidatePlot(true);
            }
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            _isBlinkOn = !_isBlinkOn;
            UpdateGearFill();
        }

        #endregion
    }
}

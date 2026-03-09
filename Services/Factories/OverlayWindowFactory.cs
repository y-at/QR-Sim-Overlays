using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels.Overlays;
using iRacing_Quick_Release.Views;
using QRO.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using iRacing_Quick_Release.Views.Overlays;

namespace QRO.Services.Factories
{
    public class OverlayWindowFactory : IOverlayWindowFactory
    {
        #region Fields

        private readonly ITelemetryServiceManager _telemetryServiceManager;

        #endregion

        #region Constructors

        public OverlayWindowFactory(ITelemetryServiceManager telemetryServiceManager)
        {
            _telemetryServiceManager = telemetryServiceManager;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates and displays a new overlay window of the specified type.
        /// </summary>
        /// <remarks>The returned window is not modal and may require additional configuration or event
        /// handling depending on the overlay type.</remarks>
        /// <param name="overlayEnum">The type of overlay to display, specified as an <see cref="OverlayName"/> Enumeration of what overlay to draw.</param>
        /// <returns>A <see cref="Window"/> instance representing the newly created overlay.</returns>
        public Window SpawnOverlay(OverlayName overlayEnum)
        {
            Window spawnedOverlay;
            switch (overlayEnum)
            {
                case OverlayName.AttitudeIndicator:
                    spawnedOverlay = SpawnAttitudeView();
                    break;
                case OverlayName.InputTraces:
                    spawnedOverlay = SpawnInputTracesView();
                    break;
                case OverlayName.WindDirectionCompass:
                    spawnedOverlay = SpawnWindDirectionCompassView();
                    break;
                default:
                    throw new ArgumentException($"Unsupported overlay type: {overlayEnum}");
            }

            return spawnedOverlay;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates and initializes a new <see cref="InputTraces"/> window.
        /// </summary>
        /// <returns>A <see cref="Window"/> instance configured to present input traces to the user.</returns>
        private Window SpawnInputTracesView()
        {
            var inputTracesWindow = new InputTraces(new InputTracesViewModel(_telemetryServiceManager))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            return inputTracesWindow;
        }

        /// <summary>
        /// Creates and initializes a new <see cref="AttitudeView"/> window.
        /// </summary>
        /// <returns>A <see cref="Window"/> instance representing the attitude view, ready to be displayed.</returns>
        private Window SpawnAttitudeView()
        {
            var attitudeWindow = new AttitudeView(new AttitudeViewModel(_telemetryServiceManager))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            return attitudeWindow;
        }

        /// <summary>
        /// Creates and initializes a new <see cref="WindDirectionCompassView"/> window.
        /// </summary>
        /// <returns>A <see cref="Window"/> instance containing the wind direction compass view, centered on the screen.</returns>
        private Window SpawnWindDirectionCompassView()
        {
            var windDirectionCompassWindow = new WindDirectionCompassView(new WindDirectionCompassViewModel(_telemetryServiceManager))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            return windDirectionCompassWindow;
        }

        #endregion
    }
}

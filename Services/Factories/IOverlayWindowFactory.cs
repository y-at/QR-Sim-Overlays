using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QRO.Enumerations;

namespace QRO.Services.Factories
{
    public interface IOverlayWindowFactory
    {
        /// <summary>
        /// Creates and displays a new overlay window of the specified type.
        /// </summary>
        /// <param name="overlayEnum">The type of overlay to display, specified as an <see cref="OverlayName"/> Enumeration of what overlay to draw.</param>
        /// <returns>A <see cref="Window"/> instance representing the newly created overlay.</returns>
        Window SpawnOverlay(OverlayName overlayEnum);
    }
}

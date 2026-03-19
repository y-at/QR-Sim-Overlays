using QRO.Enumerations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRO.Services
{
    public interface IOverlayWindowManagerService
    {
        Collection<OverlayName> CurrentOpenOverlays { get; }
        /// <summary>
        /// Displays the specified overlay on the screen.
        /// </summary>
        /// <param name="overlayEnum">The overlay to display, specified as a value of the <see cref="OverlayName"/> enumeration.</param>
        void SpawnOverlay(OverlayName overlayEnum);
        /// <summary>
        /// Closes the specified overlay.
        /// </summary>
        /// <param name="overlayEnum">The overlay to close, specified as a value of the <see cref="OverlayName"/> enumeration.</param>
        void CloseOverlay(OverlayName overlayEnum);
        /// <summary>
        /// Determines whether the specified overlay is currently open.
        /// </summary>
        /// <param name="overlayEnum">The overlay to check for an open state.</param>
        /// <returns>True if overlay window is open, false otherwise.</returns>
        bool IsOverlayOpen(OverlayName overlayEnum);
    }
}

using QRO.Enumerations;
using QRO.Models;
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

        /// <summary>
        /// Locks or unlocks every open overlay (and any overlay spawned afterward) so it either lets
        /// mouse input pass through to the sim beneath it (locked - can't be dragged or resized), or
        /// behaves normally (unlocked).
        /// </summary>
        /// <param name="locked">True to make overlays click-through and un-grabbable, false to restore normal interaction.</param>
        void SetOverlaysLocked(bool locked);

        /// <summary>
        /// Captures the current open/closed state and, for open overlays, the position and size of every known overlay.
        /// </summary>
        /// <returns>One <see cref="OverlayLayoutModel"/> per <see cref="OverlayName"/> value.</returns>
        IReadOnlyList<OverlayLayoutModel> GetCurrentLayouts();

        /// <summary>
        /// Applies a saved layout: opens the overlay (spawning it if needed) and moves/resizes it to match,
        /// or closes it if the layout marks it as not open.
        /// </summary>
        /// <param name="layout">The layout entry to apply.</param>
        void ApplyLayout(OverlayLayoutModel layout);
    }
}

using QRO.Services.Factories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QRO.Enumerations;
using QRO.Models;
using QRO.Views.Overlays;

namespace QRO.Services
{
    public class OverlayWindowManagerService : IOverlayWindowManagerService
    {
        #region Fields

        private Collection<Overlay> _overlayWindows = new();
        private readonly IOverlayWindowFactory _overlayWindowFactory;
        private bool _overlaysLocked;

        #endregion

        #region Constructors

        public OverlayWindowManagerService(IOverlayWindowFactory overlayWindowFactory)
        {
            _overlayWindowFactory = overlayWindowFactory;
        }

        #endregion

        #region Properties

        public Collection<OverlayName> CurrentOpenOverlays => new Collection<OverlayName>(_overlayWindows.Select(o => o.Name).ToList());

        #endregion

        #region Public Methods

        public void SpawnOverlay(OverlayName overlayEnum)
        {
            if (IsOverlayOpen(overlayEnum))
                return;
            Overlay spawnedOverlay = new()
            {
                Name = overlayEnum,
                Window = _overlayWindowFactory.SpawnOverlay(overlayEnum)
            };
            _overlayWindows.Add(spawnedOverlay);
            spawnedOverlay.Window.Show();

            if (_overlaysLocked)
                OverlayWindowInteraction.SetClickThrough(spawnedOverlay.Window, true);
        }

        public void CloseOverlay(OverlayName overlayEnum)
        {
            Overlay overlayToClose = _overlayWindows.FirstOrDefault(w => w.Name == overlayEnum);
            if (overlayToClose != null)
            {
                overlayToClose.Window.Close();
                _overlayWindows.Remove(overlayToClose);
                overlayToClose = null;
            }
        }
        public bool IsOverlayOpen(OverlayName overlayEnum)
        {
            return _overlayWindows.Any(o => o.Name == overlayEnum);
        }

        /// <summary>
        /// Locks or unlocks every open overlay (and any overlay spawned afterward) so it either lets
        /// mouse input pass through to the sim beneath it (locked - can't be dragged or resized), or
        /// behaves normally (unlocked).
        /// </summary>
        public void SetOverlaysLocked(bool locked)
        {
            _overlaysLocked = locked;
            foreach (var overlay in _overlayWindows)
                OverlayWindowInteraction.SetClickThrough(overlay.Window, locked);
        }

        /// <summary>
        /// Captures the current open/closed state and, for open overlays, the position and size of every known overlay.
        /// </summary>
        public IReadOnlyList<OverlayLayoutModel> GetCurrentLayouts()
        {
            return Enum.GetValues(typeof(OverlayName))
                .Cast<OverlayName>()
                .Select(name =>
                {
                    var overlay = _overlayWindows.FirstOrDefault(o => o.Name == name);
                    return new OverlayLayoutModel
                    {
                        OverlayName = name,
                        IsOpen = overlay != null,
                        Left = overlay?.Window.Left ?? 0,
                        Top = overlay?.Window.Top ?? 0,
                        Width = overlay?.Window.Width ?? 0,
                        Height = overlay?.Window.Height ?? 0,
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Applies a saved layout: opens the overlay (spawning it if needed) and moves/resizes it to match,
        /// or closes it if the layout marks it as not open.
        /// </summary>
        public void ApplyLayout(OverlayLayoutModel layout)
        {
            if (!layout.IsOpen)
            {
                CloseOverlay(layout.OverlayName);
                return;
            }

            if (!IsOverlayOpen(layout.OverlayName))
                SpawnOverlay(layout.OverlayName);

            var overlay = _overlayWindows.FirstOrDefault(o => o.Name == layout.OverlayName);
            if (overlay != null)
            {
                overlay.Window.Left = layout.Left;
                overlay.Window.Top = layout.Top;
                overlay.Window.Width = layout.Width;
                overlay.Window.Height = layout.Height;
            }
        }

        #endregion

        #region Private Methods


        #endregion

        #region Nested Classes
        private class Overlay
        {
            public Window Window;
            public OverlayName Name;
        }
        #endregion
    }
}

using QRO.Services.Factories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QRO.Enumerations;

namespace QRO.Services
{
    public class OverlayWindowManagerService : IOverlayWindowManagerService
    {
        #region Fields

        private Collection<Overlay> _overlayWindows = new();
        private readonly IOverlayWindowFactory _overlayWindowFactory;

        #endregion

        #region Constructors

        public OverlayWindowManagerService(IOverlayWindowFactory overlayWindowFactory)
        {
            _overlayWindowFactory = overlayWindowFactory;
        }

        #endregion

        #region Properties

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
        }

        public void CloseOverlay(OverlayName overlayEnum)
        {
            Overlay overlayToClose = _overlayWindows.FirstOrDefault(w => w.Name == overlayEnum);
            if (overlayToClose != null)
            {
                overlayToClose.Window.Close();
                _overlayWindows.Remove(overlayToClose);
            }
        }
        public bool IsOverlayOpen(OverlayName overlayEnum)
        {
            return _overlayWindows.Any(o => o.Name == overlayEnum);
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

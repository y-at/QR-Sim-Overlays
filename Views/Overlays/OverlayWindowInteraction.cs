using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace QRO.Views.Overlays
{
    /// <summary>
    /// Shared drag-to-resize behavior for borderless overlay windows, which have no system-provided
    /// resize chrome because they use WindowStyle="None" with AllowsTransparency="True".
    /// </summary>
    public static class OverlayWindowInteraction
    {
        #region Win32 Interop

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        /// <summary>
        /// Makes the window let mouse input pass through to whatever is beneath it (e.g. the sim), and
        /// therefore also un-grabbable for dragging/resizing, since no mouse event reaches it anymore.
        /// Also hides that window's resize grip (if it has one named "ResizeGripHandle"), since leaving
        /// a dead handle visible on screen would be misleading once it no longer does anything.
        /// </summary>
        /// <param name="window">The overlay window to lock or unlock.</param>
        /// <param name="isClickThrough">True to make the window click-through, false to restore normal interaction.</param>
        public static void SetClickThrough(Window window, bool isClickThrough)
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            SetWindowLong(hwnd, GWL_EXSTYLE, isClickThrough
                ? extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED
                : extendedStyle & ~WS_EX_TRANSPARENT);

            if (window.FindName("ResizeGripHandle") is UIElement resizeGripHandle)
                resizeGripHandle.Visibility = isClickThrough ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Wires a grip element so dragging it resizes the given window on a single linear scale —
        /// width and height always grow/shrink together in proportion to the window's own aspect
        /// ratio, rather than being dragged independently along X and Y. For a non-square window
        /// (e.g. a wide overlay like the pedal traces), the drag is projected onto that window's own
        /// diagonal, so the grip effectively "slides" along a shallower line than a 45-degree corner.
        /// </summary>
        /// <param name="window">The window to resize.</param>
        /// <param name="grip">The element the user drags to resize the window (e.g. a corner handle).</param>
        /// <param name="minWidth">The smallest width the window can be resized to.</param>
        /// <param name="minHeight">The smallest height the window can be resized to.</param>
        public static void EnableResize(Window window, FrameworkElement grip, double minWidth = 80, double minHeight = 80)
        {
            window.MinWidth = minWidth;
            window.MinHeight = minHeight;

            Point? dragStart = null;
            double startWidth = 0;
            double startHeight = 0;
            double diagonalUnitX = 0;
            double diagonalUnitY = 0;
            double diagonalLength = 0;

            grip.MouseLeftButtonDown += (s, e) =>
            {
                dragStart = grip.PointToScreen(e.GetPosition(grip));
                startWidth = window.ActualWidth;
                startHeight = window.ActualHeight;
                diagonalLength = Math.Sqrt((startWidth * startWidth) + (startHeight * startHeight));
                // Unit vector along the window's own diagonal (e.g. mostly horizontal for a wide overlay).
                diagonalUnitX = diagonalLength > 0 ? startWidth / diagonalLength : 1;
                diagonalUnitY = diagonalLength > 0 ? startHeight / diagonalLength : 0;
                grip.CaptureMouse();
                e.Handled = true;
            };

            grip.MouseMove += (s, e) =>
            {
                if (dragStart == null || e.LeftButton != MouseButtonState.Pressed || diagonalLength <= 0)
                    return;

                var current = grip.PointToScreen(e.GetPosition(grip));
                var deltaX = current.X - dragStart.Value.X;
                var deltaY = current.Y - dragStart.Value.Y;

                // Project the drag onto the diagonal to get a single scale factor, then apply it
                // uniformly to both dimensions so the aspect ratio never changes mid-drag.
                var projectedDistance = (deltaX * diagonalUnitX) + (deltaY * diagonalUnitY);
                var minScale = Math.Max(minWidth / startWidth, minHeight / startHeight);
                var scale = Math.Max(minScale, 1 + (projectedDistance / diagonalLength));

                window.Width = startWidth * scale;
                window.Height = startHeight * scale;
            };

            grip.MouseLeftButtonUp += (s, e) =>
            {
                dragStart = null;
                grip.ReleaseMouseCapture();
            };
        }
    }
}

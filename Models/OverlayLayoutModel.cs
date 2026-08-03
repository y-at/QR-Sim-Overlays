using QRO.Enumerations;

namespace QRO.Models
{
    public class OverlayLayoutModel
    {
        /// <summary>
        /// Which overlay this layout entry describes.
        /// </summary>
        public OverlayName OverlayName { get; set; }

        /// <summary>
        /// Whether the overlay should be open when this layout is applied.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Screen X position of the overlay window. Only meaningful when <see cref="IsOpen"/> is true.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Screen Y position of the overlay window. Only meaningful when <see cref="IsOpen"/> is true.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Width of the overlay window. Only meaningful when <see cref="IsOpen"/> is true.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the overlay window. Only meaningful when <see cref="IsOpen"/> is true.
        /// </summary>
        public double Height { get; set; }
    }
}

using System;

namespace QRO.Models
{
    public class FilePresetModel
    {
        /// <summary>
        /// The name of the file preset, which is also the filename without the extension. This is what is displayed to the user when selecting a preset.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The full file path to the preset file.
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// When the file was last modified.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.MinValue;
    }
}

using QRO.Enumerations;
using QRO.Models;
using System.Collections.Generic;

namespace QRO.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Returns all saved presets of the specified type, ordered by name.
        /// </summary>
        /// <param name="type">The category of preset to retrieve.</param>
        /// <returns>A read-only list of <see cref="FilePresetModel"/> instances found in the QRO appdata directory for the given type.</returns>
        IReadOnlyList<FilePresetModel> GetPresets(FilePresetType type);

        /// <summary>
        /// Copies the current iRacing game file for the specified preset type into the QRO appdata directory under the given name.
        /// </summary>
        /// <param name="type">The category of preset to save.</param>
        /// <param name="name">The file name (without extension) to use for the stored preset.</param>
        /// <exception cref="FileNotFoundException">Thrown when the source game file does not exist.</exception>
        void SavePreset(FilePresetType type, string name);

        /// <summary>
        /// Copies a saved preset from the QRO appdata directory back into the iRacing documents folder,
        /// renaming it to the filename required by the game.
        /// </summary>
        /// <param name="type">The category of preset to load.</param>
        /// <param name="name">The name of the preset to load.</param>
        /// <exception cref="FileNotFoundException">Thrown when the named preset file does not exist.</exception>
        void LoadPreset(FilePresetType type, string name);

        /// <summary>
        /// Removes the named preset file from the QRO appdata directory. Does nothing if the file does not exist.
        /// </summary>
        /// <param name="type">The category of preset to delete.</param>
        /// <param name="name">The name of the preset to delete.</param>
        void DeletePreset(FilePresetType type, string name);

        /// <summary>
        /// Compares the current iRacing game file against all saved presets of the specified type.
        /// </summary>
        /// <param name="type">The category of preset to check against.</param>
        /// <returns>
        /// <see langword="true"/> if the game file matches an existing preset or does not exist;
        /// <see langword="false"/> if the file exists but does not match any saved preset.
        /// </returns>
        bool CurrentFileMatchesAnyPreset(FilePresetType type);

        /// <summary>
        /// Determines whether the iRacing game file for the specified preset type exists on disk.
        /// </summary>
        /// <param name="type">The preset type whose corresponding game file to check.</param>
        /// <returns><see langword="true"/> if the game file is present; otherwise, <see langword="false"/>.</returns>
        bool GameFileExists(FilePresetType type);

        /// <summary>
        /// Determines whether a preset with the given name already exists in the QRO appdata directory.
        /// </summary>
        /// <param name="type">The category of preset to check.</param>
        /// <param name="name">The preset name to look up.</param>
        /// <returns><see langword="true"/> if a matching preset file exists; otherwise, <see langword="false"/>.</returns>
        bool PresetExists(FilePresetType type, string name);
    }
}

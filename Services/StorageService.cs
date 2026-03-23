using QRO.Enumerations;
using QRO.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QRO.Services
{
    public class StorageService : IStorageService
    {
        #region Fields

        private static readonly string AppDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRO", "Presets");

        private static readonly string iRacingDocPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "iRacing");

        #endregion

        #region Constructors

        public StorageService()
        {

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns all saved presets of the specified type from the QRO appdata directory, ordered alphabetically by name. Returns an empty list if the preset directory does not yet exist.
        /// </summary>
        /// <param name="type">The category of preset to retrieve.</param>
        /// <returns>A read-only list of <see cref="FilePresetModel"/> instances found on disk.</returns>
        public IReadOnlyList<FilePresetModel> GetPresets(FilePresetType type)
        {
            var directory = GetPresetDirectory(type);

            if (!Directory.Exists(directory))
                return Array.Empty<FilePresetModel>();

            return Directory.GetFiles(directory).Select
            (file => new FilePresetModel {
                Name = Path.GetFileNameWithoutExtension(file), FilePath = file, LastModified = File.GetLastWriteTime(file)}).OrderBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Copies the current iRacing game file into the QRO appdata preset directory under the given name, creating the directory if it does not exist. Overwrites any existing preset with the same name.
        /// </summary>
        /// <param name="type">The category of preset to save.</param>
        /// <param name="name">The file name (without extension) to use for the stored preset.</param>
        /// <exception cref="FileNotFoundException">Thrown when the source game file does not exist in the iRacing documents folder.</exception>
        public void SavePreset(FilePresetType type, string name)
        {
            var gameFile = GetGameFilePath(type);

            if (!File.Exists(gameFile))
                throw new FileNotFoundException($"Game file not found: {gameFile}");

            var directory = GetPresetDirectory(type);
            Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(GetGameFileName(type));
            var destPath = Path.Combine(directory, name + extension);
            File.Copy(gameFile, destPath, overwrite: true);
        }

        /// <summary>
        /// Copies a named preset from the QRO appdata directory into the iRacing documents folder, renaming it to the filename required by the game (e.g. controls.cfg, RendererDX11Monitor.ini).
        /// </summary>
        /// <param name="type">The category of preset to load.</param>
        /// <param name="name">The name of the preset to load.</param>
        /// <exception cref="FileNotFoundException">Thrown when no preset with the given name exists for the specified type.</exception>
        public void LoadPreset(FilePresetType type, string name)
        {
            var directory = GetPresetDirectory(type);
            var extension = Path.GetExtension(GetGameFileName(type));
            var srcPath = Path.Combine(directory, name + extension);

            if (!File.Exists(srcPath))
                throw new FileNotFoundException($"Preset file not found: {srcPath}");

            Directory.CreateDirectory(iRacingDocPath);
            File.Copy(srcPath, GetGameFilePath(type), overwrite: true);
        }

        /// <summary>
        /// Removes the named preset file from the QRO appdata directory. Does nothing if the file does not exist.
        /// </summary>
        /// <param name="type">The category of preset to delete.</param>
        /// <param name="name">The name of the preset to delete.</param>
        public void DeletePreset(FilePresetType type, string name)
        {
            var directory = GetPresetDirectory(type);
            var extension = Path.GetExtension(GetGameFileName(type));
            var filePath = Path.Combine(directory, name + extension);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Performs a byte-by-byte comparison of the current iRacing game file against all saved presets of the specified type. Returns True if no game file is present (nothing to back up) or if a matching preset already exists.
        /// </summary>
        /// <param name="type">The preset category whose game file and saved presets to compare.</param>
        /// <returns>True if the game file is absent or matches an existing preset, false if the game file differs from all saved presets.</returns>
        public bool CurrentFileMatchesAnyPreset(FilePresetType type)
        {
            var gameFile = GetGameFilePath(type);
            if (!File.Exists(gameFile))
                return true;

            var directory = GetPresetDirectory(type);
            if (!Directory.Exists(directory))
                return false;

            return Directory.GetFiles(directory).Any(p => FilesAreEqual(gameFile, p));
        }

        /// <summary>
        /// Determines whether the iRacing game file corresponding to the specified preset type exists in the Documents\iRacing folder.
        /// </summary>
        /// <param name="type">The preset type whose game file to check.</param>
        /// <returns>True if the file is present on disk, otherwise false.</returns>
        public bool GameFileExists(FilePresetType type)
        {
            return File.Exists(GetGameFilePath(type));
        }

        /// <summary>
        /// Determines whether a preset with the given name already exists in the QRO appdata directory for the specified type.
        /// </summary>
        /// <param name="type">The category of preset to check.</param>
        /// <param name="name">The preset name (without extension) to look up.</param>
        /// <returns>True if a matching preset file is found; otherwise, false.</returns>
        public bool PresetExists(FilePresetType type, string name)
        {
            var directory = GetPresetDirectory(type);
            var extension = Path.GetExtension(GetGameFileName(type));
            return File.Exists(Path.Combine(directory, name + extension));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the QRO appdata directory path for the given preset type.
        /// </summary>
        private string GetPresetDirectory(FilePresetType type)
        {
            switch(type)
            {
                case FilePresetType.Controls:
                    return Path.Combine(AppDataRoot, "Controls");
                case FilePresetType.GraphicsMonitor:
                    return Path.Combine(AppDataRoot, "Graphics", "Monitor");
                case FilePresetType.GraphicsVR:
                    return Path.Combine(AppDataRoot, "Graphics", "VR");
                default:
                    throw new ArgumentException($"{nameof(StorageService)} in {nameof(GetPresetDirectory)} FilePresetType of {nameof(type)} does not exist as available option.");
            }
        }

        /// <summary>
        /// Returns the filename (with extension) that iRacing expects for the given preset type.
        /// </summary>
        private string GetGameFileName(FilePresetType type)
        {
            switch(type)
            {
                case FilePresetType.Controls:
                    return "controls.cfg";
                case FilePresetType.GraphicsMonitor:
                    return "rendererDX11OpenXR.ini";
                case FilePresetType.GraphicsVR:
                    return "rendererDX11OpenXR.ini";
                default:
                    throw new ArgumentException($"{nameof(StorageService)} in {nameof(GetGameFileName)} FilePresetType of {nameof(type)} does not exist as available option." );
            }
        }

        /// <summary>
        /// Returns the full path to the iRacing game file for the given preset type.
        /// </summary>
        private string GetGameFilePath(FilePresetType type)
        {
            return Path.Combine(iRacingDocPath, GetGameFileName(type));
        }

        /// <summary>
        /// Performs a byte-by-byte comparison of two files. Returns false immediately if their sizes differ, avoiding a full read when possible.
        /// </summary>
        /// <param name="path1">Absolute path to the first file.</param>
        /// <param name="path2">Absolute path to the second file.</param>
        /// <returns>True if both files have identical contents; otherwise, false.</returns>
        private static bool FilesAreEqual(string path1, string path2)
        {
            var info1 = new FileInfo(path1);
            var info2 = new FileInfo(path2);
            if (info1.Length != info2.Length)
                return false;

            using var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);

            const int bufferSize = 4096;
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            int read;
            while ((read = fs1.Read(buffer1, 0, bufferSize)) > 0)
            {
                fs2.Read(buffer2, 0, bufferSize);
                for (int i = 0; i < read; i++)
                {
                    if (buffer1[i] != buffer1[i])
                        return false;
                }
            }
            return true;
        }

        #endregion
    }
}

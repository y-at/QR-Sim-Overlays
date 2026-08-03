using QRO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QRO.Services
{
    public class OverlayProfileService : IOverlayProfileService
    {
        #region Fields

        private static readonly string ProfilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRO", "OverlayProfiles");

        private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRO", "Settings");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "overlay-settings.json");

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns all saved overlay profiles, ordered by name. Returns an empty list if none have been saved yet.
        /// </summary>
        public IReadOnlyList<FilePresetModel> GetProfiles()
        {
            if (!Directory.Exists(ProfilesDirectory))
                return Array.Empty<FilePresetModel>();

            return Directory.GetFiles(ProfilesDirectory, "*.json")
                .Select(file => new FilePresetModel
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                    LastModified = File.GetLastWriteTime(file),
                })
                .OrderBy(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// Saves the given overlay layouts as a named profile, creating the profile directory if needed and overwriting any existing profile with the same name.
        /// </summary>
        public void SaveProfile(string name, IEnumerable<OverlayLayoutModel> layouts)
        {
            Directory.CreateDirectory(ProfilesDirectory);
            var json = JsonSerializer.Serialize(layouts.ToList());
            File.WriteAllText(GetProfilePath(name), json);
        }

        /// <summary>
        /// Loads the layout entries for the named profile.
        /// </summary>
        public IReadOnlyList<OverlayLayoutModel> LoadProfile(string name)
        {
            var path = GetProfilePath(name);
            if (!File.Exists(path))
                return Array.Empty<OverlayLayoutModel>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<OverlayLayoutModel>>(json) ?? new List<OverlayLayoutModel>();
            }
            catch (JsonException)
            {
                return Array.Empty<OverlayLayoutModel>();
            }
        }

        /// <summary>
        /// Removes the named profile. Does nothing if it does not exist.
        /// </summary>
        public void DeleteProfile(string name)
        {
            var path = GetProfilePath(name);
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Renames a saved profile.
        /// </summary>
        public void RenameProfile(string oldName, string newName)
        {
            var oldPath = GetProfilePath(oldName);
            var newPath = GetProfilePath(newName);

            if (!File.Exists(oldPath))
                throw new FileNotFoundException($"Profile file not found: {oldPath}");

            if (File.Exists(newPath))
                throw new IOException($"A profile named \"{newName}\" already exists.");

            File.Move(oldPath, newPath);
        }

        /// <summary>
        /// Determines whether a profile with the given name already exists.
        /// </summary>
        public bool ProfileExists(string name) => File.Exists(GetProfilePath(name));

        /// <summary>
        /// Whether a profile should be automatically applied when the simulator connects.
        /// </summary>
        public bool GetAutoLoadEnabled() => LoadSettings().AutoLoadEnabled;

        /// <summary>
        /// Persists whether a profile should be automatically applied when the simulator connects.
        /// </summary>
        public void SetAutoLoadEnabled(bool enabled)
        {
            var settings = LoadSettings();
            settings.AutoLoadEnabled = enabled;
            SaveSettings(settings);
        }

        /// <summary>
        /// The name of the profile to apply automatically when the simulator connects, or null if none is set.
        /// </summary>
        public string GetAutoLoadProfileName() => LoadSettings().AutoLoadProfileName;

        /// <summary>
        /// Persists the name of the profile to apply automatically when the simulator connects.
        /// </summary>
        public void SetAutoLoadProfileName(string profileName)
        {
            var settings = LoadSettings();
            settings.AutoLoadProfileName = profileName;
            SaveSettings(settings);
        }

        /// <summary>
        /// Whether overlays should be locked (click-through, un-grabbable) by default.
        /// </summary>
        public bool GetLockOverlaysEnabled() => LoadSettings().LockOverlaysEnabled;

        /// <summary>
        /// Persists whether overlays should be locked (click-through, un-grabbable) by default.
        /// </summary>
        public void SetLockOverlaysEnabled(bool enabled)
        {
            var settings = LoadSettings();
            settings.LockOverlaysEnabled = enabled;
            SaveSettings(settings);
        }

        /// <summary>
        /// Whether open overlays should automatically close when the simulator disconnects.
        /// </summary>
        public bool GetUnloadOnDisconnectEnabled() => LoadSettings().UnloadOnDisconnectEnabled;

        /// <summary>
        /// Persists whether open overlays should automatically close when the simulator disconnects.
        /// </summary>
        public void SetUnloadOnDisconnectEnabled(bool enabled)
        {
            var settings = LoadSettings();
            settings.UnloadOnDisconnectEnabled = enabled;
            SaveSettings(settings);
        }

        #endregion

        #region Private Methods

        private string GetProfilePath(string name) => Path.Combine(ProfilesDirectory, name + ".json");

        private OverlaySettings LoadSettings()
        {
            if (!File.Exists(SettingsFilePath))
                return new OverlaySettings();

            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<OverlaySettings>(json) ?? new OverlaySettings();
            }
            catch (JsonException)
            {
                return new OverlaySettings();
            }
        }

        private void SaveSettings(OverlaySettings settings)
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(SettingsFilePath, json);
        }

        #endregion

        #region Nested Types

        private class OverlaySettings
        {
            public bool AutoLoadEnabled { get; set; }
            public string AutoLoadProfileName { get; set; }
            public bool LockOverlaysEnabled { get; set; }
            public bool UnloadOnDisconnectEnabled { get; set; }
        }

        #endregion
    }
}

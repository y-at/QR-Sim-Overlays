using QRO.Models;
using System.Collections.Generic;
using System.IO;

namespace QRO.Services
{
    public interface IOverlayProfileService
    {
        /// <summary>
        /// Returns all saved overlay profiles, ordered by name.
        /// </summary>
        IReadOnlyList<FilePresetModel> GetProfiles();

        /// <summary>
        /// Saves the given overlay layouts as a named profile, overwriting any existing profile with the same name.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <param name="layouts">The per-overlay layout entries to save.</param>
        void SaveProfile(string name, IEnumerable<OverlayLayoutModel> layouts);

        /// <summary>
        /// Loads the layout entries for the named profile.
        /// </summary>
        /// <param name="name">The name of the profile to load.</param>
        /// <returns>The saved layout entries, or an empty list if the profile does not exist or could not be read.</returns>
        IReadOnlyList<OverlayLayoutModel> LoadProfile(string name);

        /// <summary>
        /// Removes the named profile. Does nothing if it does not exist.
        /// </summary>
        void DeleteProfile(string name);

        /// <summary>
        /// Renames a saved profile.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when no profile with <paramref name="oldName"/> exists.</exception>
        /// <exception cref="IOException">Thrown when a profile named <paramref name="newName"/> already exists.</exception>
        void RenameProfile(string oldName, string newName);

        /// <summary>
        /// Determines whether a profile with the given name already exists.
        /// </summary>
        bool ProfileExists(string name);

        /// <summary>
        /// Whether a profile should be automatically applied when the simulator connects.
        /// </summary>
        bool GetAutoLoadEnabled();

        /// <summary>
        /// Persists whether a profile should be automatically applied when the simulator connects.
        /// </summary>
        void SetAutoLoadEnabled(bool enabled);

        /// <summary>
        /// The name of the profile to apply automatically when the simulator connects, or null if none is set.
        /// </summary>
        string GetAutoLoadProfileName();

        /// <summary>
        /// Persists the name of the profile to apply automatically when the simulator connects.
        /// </summary>
        void SetAutoLoadProfileName(string profileName);

        /// <summary>
        /// Whether overlays should be locked (click-through, un-grabbable) by default.
        /// </summary>
        bool GetLockOverlaysEnabled();

        /// <summary>
        /// Persists whether overlays should be locked (click-through, un-grabbable) by default.
        /// </summary>
        void SetLockOverlaysEnabled(bool enabled);

        /// <summary>
        /// Whether open overlays should automatically close when the simulator disconnects.
        /// </summary>
        bool GetUnloadOnDisconnectEnabled();

        /// <summary>
        /// Persists whether open overlays should automatically close when the simulator disconnects.
        /// </summary>
        void SetUnloadOnDisconnectEnabled(bool enabled);
    }
}

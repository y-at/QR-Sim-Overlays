using QRO.Enumerations;

namespace QRO.Services
{
    public interface IThemeService
    {
        /// <summary>
        /// The currently applied theme.
        /// </summary>
        AppTheme CurrentTheme { get; }

        /// <summary>
        /// Applies and persists the given theme, swapping the app's theme resource dictionary.
        /// </summary>
        void ApplyTheme(AppTheme theme);

        /// <summary>
        /// Switches between light and dark theme.
        /// </summary>
        void ToggleTheme();

        /// <summary>
        /// Loads the user's previously saved theme, defaulting to <see cref="AppTheme.Dark"/> if none has been saved.
        /// </summary>
        AppTheme LoadSavedTheme();
    }
}

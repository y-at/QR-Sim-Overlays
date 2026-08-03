using QRO.Enumerations;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace QRO.Services
{
    public class ThemeService : IThemeService
    {
        #region Fields

        private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRO", "Settings");

        private static readonly string ThemeFilePath = Path.Combine(SettingsDirectory, "theme.txt");

        private const string ThemeDictionaryPathFragment = "Resources/Themes/";

        private AppTheme _currentTheme;

        #endregion

        #region Properties

        /// <summary>
        /// The currently applied theme.
        /// </summary>
        public AppTheme CurrentTheme => _currentTheme;

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies and persists the given theme, swapping the app's theme resource dictionary.
        /// </summary>
        public void ApplyTheme(AppTheme theme)
        {
            var themeUri = new Uri($"{ThemeDictionaryPathFragment}{(theme == AppTheme.Dark ? "DarkTheme" : "LightTheme")}.xaml", UriKind.Relative);
            var newDictionary = new ResourceDictionary { Source = themeUri };

            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            var existingThemeDictionary = mergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains(ThemeDictionaryPathFragment));

            if (existingThemeDictionary != null)
                mergedDictionaries.Remove(existingThemeDictionary);

            mergedDictionaries.Add(newDictionary);

            _currentTheme = theme;
            SaveTheme(theme);
        }

        /// <summary>
        /// Switches between light and dark theme.
        /// </summary>
        public void ToggleTheme()
        {
            ApplyTheme(_currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
        }

        /// <summary>
        /// Loads the user's previously saved theme, defaulting to <see cref="AppTheme.Dark"/> if none has been saved.
        /// </summary>
        public AppTheme LoadSavedTheme()
        {
            if (File.Exists(ThemeFilePath) && Enum.TryParse<AppTheme>(File.ReadAllText(ThemeFilePath).Trim(), out var savedTheme))
                return savedTheme;

            return AppTheme.Dark;
        }

        #endregion

        #region Private Methods

        private void SaveTheme(AppTheme theme)
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(ThemeFilePath, theme.ToString());
        }

        #endregion
    }
}

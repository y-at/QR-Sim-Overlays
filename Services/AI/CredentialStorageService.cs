using QRO.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace QRO.Services.AI
{
    public class CredentialStorageService : ICredentialStorageService
    {
        #region Fields

        private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QRO", "Settings");

        private static readonly string CredentialsFilePath = Path.Combine(SettingsDirectory, "credentials.json");

        private static readonly byte[] Entropy = { 0x51, 0x52, 0x4F, 0x2D, 0x41, 0x49 }; // "QRO-AI"

        #endregion

        #region Public Methods

        /// <summary>
        /// Encrypts and persists the API key for the given provider, overwriting any existing key.
        /// </summary>
        public void SaveApiKey(AIProvider provider, string apiKey)
        {
            var store = LoadStore();
            var protectedBytes = ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(apiKey ?? string.Empty), Entropy, DataProtectionScope.CurrentUser);
            store.EncryptedKeys[provider.ToString()] = Convert.ToBase64String(protectedBytes);
            SaveStore(store);
        }

        /// <summary>
        /// Attempts to retrieve and decrypt the stored API key for the given provider.
        /// </summary>
        public bool TryGetApiKey(AIProvider provider, out string apiKey)
        {
            apiKey = null;
            var store = LoadStore();

            if (!store.EncryptedKeys.TryGetValue(provider.ToString(), out var encoded) || string.IsNullOrEmpty(encoded))
                return false;

            try
            {
                var protectedBytes = Convert.FromBase64String(encoded);
                var plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                apiKey = System.Text.Encoding.UTF8.GetString(plainBytes);
                return !string.IsNullOrEmpty(apiKey);
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether an API key has been stored for the given provider.
        /// </summary>
        public bool HasApiKey(AIProvider provider)
        {
            var store = LoadStore();
            return store.EncryptedKeys.ContainsKey(provider.ToString());
        }

        /// <summary>
        /// Removes the stored API key for the given provider. Does nothing if none is stored.
        /// </summary>
        public void ClearApiKey(AIProvider provider)
        {
            var store = LoadStore();
            if (store.EncryptedKeys.Remove(provider.ToString()))
                SaveStore(store);
        }

        /// <summary>
        /// Gets the user's last-selected AI provider, defaulting to <see cref="AIProvider.OpenAI"/> if none has been set.
        /// </summary>
        public AIProvider GetSelectedProvider()
        {
            var store = LoadStore();
            if (Enum.TryParse<AIProvider>(store.SelectedProvider, out var provider))
                return provider;

            return AIProvider.OpenAI;
        }

        /// <summary>
        /// Persists the user's selected AI provider.
        /// </summary>
        public void SetSelectedProvider(AIProvider provider)
        {
            var store = LoadStore();
            store.SelectedProvider = provider.ToString();
            SaveStore(store);
        }

        #endregion

        #region Private Methods

        private CredentialStoreData LoadStore()
        {
            if (!File.Exists(CredentialsFilePath))
                return new CredentialStoreData();

            try
            {
                var json = File.ReadAllText(CredentialsFilePath);
                return JsonSerializer.Deserialize<CredentialStoreData>(json) ?? new CredentialStoreData();
            }
            catch (JsonException)
            {
                return new CredentialStoreData();
            }
        }

        private void SaveStore(CredentialStoreData store)
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(store);
            File.WriteAllText(CredentialsFilePath, json);
        }

        #endregion

        #region Nested Types

        private class CredentialStoreData
        {
            public string SelectedProvider { get; set; }
            public Dictionary<string, string> EncryptedKeys { get; set; } = new Dictionary<string, string>();
        }

        #endregion
    }
}

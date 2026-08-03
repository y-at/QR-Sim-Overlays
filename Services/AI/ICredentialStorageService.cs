using QRO.Enumerations;

namespace QRO.Services.AI
{
    public interface ICredentialStorageService
    {
        /// <summary>
        /// Encrypts and persists the API key for the given provider, overwriting any existing key.
        /// </summary>
        void SaveApiKey(AIProvider provider, string apiKey);

        /// <summary>
        /// Attempts to retrieve and decrypt the stored API key for the given provider.
        /// </summary>
        /// <returns><see langword="true"/> if a key was found and decrypted successfully; otherwise <see langword="false"/>.</returns>
        bool TryGetApiKey(AIProvider provider, out string apiKey);

        /// <summary>
        /// Determines whether an API key has been stored for the given provider.
        /// </summary>
        bool HasApiKey(AIProvider provider);

        /// <summary>
        /// Removes the stored API key for the given provider. Does nothing if none is stored.
        /// </summary>
        void ClearApiKey(AIProvider provider);

        /// <summary>
        /// Gets the user's last-selected AI provider, defaulting to <see cref="AIProvider.OpenAI"/> if none has been set.
        /// </summary>
        AIProvider GetSelectedProvider();

        /// <summary>
        /// Persists the user's selected AI provider.
        /// </summary>
        void SetSelectedProvider(AIProvider provider);
    }
}

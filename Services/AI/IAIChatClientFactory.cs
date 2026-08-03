using QRO.Enumerations;

namespace QRO.Services.AI
{
    public interface IAIChatClientFactory
    {
        /// <summary>
        /// Returns the chat client implementation for the given provider.
        /// </summary>
        IAIChatClient GetClient(AIProvider provider);
    }
}

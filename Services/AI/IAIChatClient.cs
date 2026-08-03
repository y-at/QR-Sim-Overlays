using QRO.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QRO.Services.AI
{
    public interface IAIChatClient
    {
        /// <summary>
        /// Sends the conversation history to the provider and returns the assistant's reply.
        /// </summary>
        /// <param name="systemPrompt">System-level instructions and live session context.</param>
        /// <param name="history">Prior user/assistant turns, oldest first.</param>
        /// <param name="apiKey">The user's API key for this provider.</param>
        /// <exception cref="AIChatException">Thrown when the request fails or the provider returns an error.</exception>
        Task<string> SendAsync(string systemPrompt, IReadOnlyList<ChatMessageModel> history, string apiKey, CancellationToken cancellationToken);
    }
}

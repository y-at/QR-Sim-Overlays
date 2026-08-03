using Anthropic;
using Anthropic.Models.Messages;
using QRO.Enumerations;
using QRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QRO.Services.AI
{
    public class AnthropicChatClient : IAIChatClient
    {
        #region Fields

        private const string Model = "claude-sonnet-5";
        private const int MaxTokens = 1024;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends the conversation to Anthropic's Messages API and returns the assistant's reply.
        /// </summary>
        public async Task<string> SendAsync(string systemPrompt, IReadOnlyList<ChatMessageModel> history, string apiKey, CancellationToken cancellationToken)
        {
            var client = new AnthropicClient { ApiKey = apiKey };

            var parameters = new MessageCreateParams
            {
                Model = Model,
                MaxTokens = MaxTokens,
                System = systemPrompt,
                Messages = history
                    .Where(m => m.Role != ChatRole.System)
                    .Select(m => new MessageParam
                    {
                        Role = m.Role == ChatRole.Assistant ? Role.Assistant : Role.User,
                        Content = m.Content,
                    })
                    .ToList(),
            };

            Message response;
            try
            {
                response = await client.Messages.Create(parameters);
            }
            catch (Exception ex)
            {
                throw new AIChatException($"Anthropic request failed: {ex.Message}", ex);
            }

            var text = response.Content
                .Select(block => block.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text;

            return text ?? string.Empty;
        }

        #endregion
    }
}

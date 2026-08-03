using QRO.Enumerations;
using System;

namespace QRO.Services.AI
{
    public class AIChatClientFactory : IAIChatClientFactory
    {
        #region Fields

        private readonly OpenAIChatClient _openAIChatClient;
        private readonly AnthropicChatClient _anthropicChatClient;

        #endregion

        #region Constructors

        public AIChatClientFactory(OpenAIChatClient openAIChatClient, AnthropicChatClient anthropicChatClient)
        {
            _openAIChatClient = openAIChatClient;
            _anthropicChatClient = anthropicChatClient;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the chat client implementation for the given provider.
        /// </summary>
        public IAIChatClient GetClient(AIProvider provider)
        {
            switch (provider)
            {
                case AIProvider.OpenAI:
                    return _openAIChatClient;
                case AIProvider.Anthropic:
                    return _anthropicChatClient;
                default:
                    throw new ArgumentException($"{nameof(AIChatClientFactory)} in {nameof(GetClient)}: AIProvider of {nameof(provider)} does not exist as available option.");
            }
        }

        #endregion
    }
}

using QRO.Enumerations;
using QRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QRO.Services.AI
{
    public class OpenAIChatClient : IAIChatClient
    {
        #region Fields

        // Update this if OpenAI retires the model - user-supplied key, no other config surface yet.
        private const string Model = "gpt-4o";
        private const string Endpoint = "https://api.openai.com/v1/chat/completions";

        private readonly HttpClient _httpClient;

        #endregion

        #region Constructors

        public OpenAIChatClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends the conversation to OpenAI's Chat Completions API and returns the assistant's reply.
        /// </summary>
        public async Task<string> SendAsync(string systemPrompt, IReadOnlyList<ChatMessageModel> history, string apiKey, CancellationToken cancellationToken)
        {
            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            messages.AddRange(history
                .Where(m => m.Role != ChatRole.System)
                .Select(m => (object)new { role = RoleToString(m.Role), content = m.Content }));

            var payload = new { model = Model, messages };

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new AIChatException("Could not reach OpenAI. Check your internet connection.", ex);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new AIChatException($"OpenAI request failed ({(int)response.StatusCode}): {ExtractErrorMessage(body)}");

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }

        #endregion

        #region Private Methods

        private static string RoleToString(ChatRole role)
        {
            switch (role)
            {
                case ChatRole.Assistant:
                    return "assistant";
                case ChatRole.System:
                    return "system";
                default:
                    return "user";
            }
        }

        private static string ExtractErrorMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var message))
                    return message.GetString();
            }
            catch (JsonException)
            {
                // Fall through and return the raw body below.
            }
            return body;
        }

        #endregion
    }
}

using Prism.Commands;
using QRO.Enumerations;
using QRO.Models;
using QRO.Services;
using QRO.Services.AI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QRO.ViewModels
{
    public class RaceEngineerChatViewModel : ViewModelBase
    {
        #region Fields

        private const string BaselineSetupPrompt = "Give me a baseline setup for this car and track, taking the current conditions and my rating into account.";

        private readonly ITelemetryServiceManager _telemetryServiceManager;
        private readonly IAIChatClientFactory _chatClientFactory;
        private readonly ICredentialStorageService _credentialStorageService;
        private readonly IRaceEngineerContextBuilder _contextBuilder;

        private SessionModel _latestSession;

        private string _inputText;
        private bool _isSending;
        private string _statusMessage;
        private AIProvider _selectedProvider;
        private string _apiKeyInput;
        private bool _hasApiKeyForSelectedProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Conversation history shown in the chat view.
        /// </summary>
        public ObservableCollection<ChatMessageModel> Messages { get; } = new ObservableCollection<ChatMessageModel>();

        /// <summary>
        /// AI providers the user can pick between.
        /// </summary>
        public IEnumerable<AIProvider> AvailableProviders { get; } = Enum.GetValues(typeof(AIProvider)).Cast<AIProvider>().ToList();

        /// <summary>
        /// Text currently typed into the chat input box.
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set
            {
                if (SetProperty(ref _inputText, value))
                    SendMessageCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// True while waiting for a reply from the AI provider.
        /// </summary>
        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (SetProperty(ref _isSending, value))
                {
                    SendMessageCommand.RaiseCanExecuteChanged();
                    RequestBaselineSetupCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Status or error text shown below the chat input.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// The AI provider the user has selected.
        /// </summary>
        public AIProvider SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                if (SetProperty(ref _selectedProvider, value))
                {
                    _credentialStorageService.SetSelectedProvider(value);
                    RefreshHasApiKey();
                }
            }
        }

        /// <summary>
        /// Write-only staging field for the API key entry box.
        /// </summary>
        public string ApiKeyInput
        {
            get => _apiKeyInput;
            set => SetProperty(ref _apiKeyInput, value);
        }

        /// <summary>
        /// Whether an API key is already stored for <see cref="SelectedProvider"/>.
        /// </summary>
        public bool HasApiKeyForSelectedProvider
        {
            get => _hasApiKeyForSelectedProvider;
            private set
            {
                if (SetProperty(ref _hasApiKeyForSelectedProvider, value))
                {
                    SendMessageCommand.RaiseCanExecuteChanged();
                    RequestBaselineSetupCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand SendMessageCommand { get; }
        public DelegateCommand SaveApiKeyCommand { get; }
        public DelegateCommand RequestBaselineSetupCommand { get; }

        #endregion

        #region Constructors

        public RaceEngineerChatViewModel(
            ITelemetryServiceManager telemetryServiceManager,
            IAIChatClientFactory chatClientFactory,
            ICredentialStorageService credentialStorageService,
            IRaceEngineerContextBuilder contextBuilder)
        {
            _telemetryServiceManager = telemetryServiceManager;
            _chatClientFactory = chatClientFactory;
            _credentialStorageService = credentialStorageService;
            _contextBuilder = contextBuilder;

            _telemetryServiceManager.DataReceived += OnDataReceived;

            SendMessageCommand = new DelegateCommand(async () => await SendMessageAsync(InputText), CanSendMessage);
            SaveApiKeyCommand = new DelegateCommand(SaveApiKey);
            RequestBaselineSetupCommand = new DelegateCommand(async () => await SendMessageAsync(BaselineSetupPrompt), CanSendMessage);

            _selectedProvider = _credentialStorageService.GetSelectedProvider();
            RefreshHasApiKey();
        }

        #endregion

        #region Private Methods

        private bool CanSendMessage() => !IsSending && HasApiKeyForSelectedProvider;

        private void SaveApiKey()
        {
            if (string.IsNullOrWhiteSpace(ApiKeyInput))
            {
                StatusMessage = "Please enter an API key before saving.";
                return;
            }

            _credentialStorageService.SaveApiKey(SelectedProvider, ApiKeyInput.Trim());
            ApiKeyInput = string.Empty;
            StatusMessage = $"API key saved for {SelectedProvider}.";
            RefreshHasApiKey();
        }

        private void RefreshHasApiKey()
        {
            HasApiKeyForSelectedProvider = _credentialStorageService.HasApiKey(SelectedProvider);
            if (!HasApiKeyForSelectedProvider)
                StatusMessage = $"Add an API key for {SelectedProvider} to start chatting.";
        }

        private async Task SendMessageAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || IsSending)
                return;

            if (!_credentialStorageService.TryGetApiKey(SelectedProvider, out var apiKey))
            {
                StatusMessage = $"No API key stored for {SelectedProvider}.";
                return;
            }

            Messages.Add(new ChatMessageModel(ChatRole.User, text.Trim()));
            InputText = string.Empty;

            IsSending = true;
            StatusMessage = null;

            try
            {
                var systemPrompt = _contextBuilder.BuildSystemPrompt(_latestSession);
                var client = _chatClientFactory.GetClient(SelectedProvider);
                var reply = await client.SendAsync(systemPrompt, Messages.ToList(), apiKey, CancellationToken.None);
                Messages.Add(new ChatMessageModel(ChatRole.Assistant, reply));
            }
            catch (AIChatException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                IsSending = false;
            }
        }

        #endregion

        #region Event Listeners

        private void OnDataReceived(SessionModel session)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => OnDataReceived(session));
                return;
            }

            if (IsDisposed)
                return;

            _latestSession = session;
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_telemetryServiceManager != null)
                {
                    _telemetryServiceManager.DataReceived -= OnDataReceived;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}

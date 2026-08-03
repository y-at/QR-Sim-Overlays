using QRO.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QRO.Views
{
    /// <summary>
    /// Interaction logic for RaceEngineerChatView.xaml
    /// </summary>
    public partial class RaceEngineerChatView : UserControl
    {
        public RaceEngineerChatView()
        {
            InitializeComponent();
            DataContextChanged += RaceEngineerChatView_DataContextChanged;
        }

        private void RaceEngineerChatView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RaceEngineerChatViewModel oldViewModel)
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is RaceEngineerChatViewModel newViewModel)
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // PasswordBox.Password isn't bindable; clear it whenever the ViewModel's staging
            // field is cleared (after a successful save) so the box doesn't keep showing the old key.
            if (e.PropertyName == nameof(RaceEngineerChatViewModel.ApiKeyInput) &&
                sender is RaceEngineerChatViewModel viewModel &&
                string.IsNullOrEmpty(viewModel.ApiKeyInput))
            {
                ApiKeyPasswordBox.Password = string.Empty;
            }
        }

        private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaceEngineerChatViewModel viewModel)
                viewModel.ApiKeyInput = ApiKeyPasswordBox.Password;
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is RaceEngineerChatViewModel viewModel && viewModel.SendMessageCommand.CanExecute())
                viewModel.SendMessageCommand.Execute();

            e.Handled = true;
        }
    }
}

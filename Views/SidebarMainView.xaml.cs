using QRO.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QRO.Views
{
    /// <summary>
    /// Interaction logic for SidebarMainView.xaml
    /// </summary>
    public partial class SidebarMainView : UserControl
    {
        #region Fields

        private SidebarMainViewModel? _viewModel => DataContext as SidebarMainViewModel;

        #endregion

        #region

        public SidebarMainView()
        {
            InitializeComponent();
        }

        #endregion

        private void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnOverlayTabSelected();
        }

        private void ControlsPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnControlsPresetsTabSelected();
        }

        private void GraphicsPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnGraphicsPresetsTabSelected();
        }

        private void RaceEngineerButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnRaceEngineerTabSelected();
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnGithubButtonPress();
        }
    }
}

using QRO.ViewModels.Overlays;
using QRO.Views;
using QRO.Views.Overlays;
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
using QRO.ViewModels;

namespace QRO.Views
{
    /// <summary>
    /// Interaction logic for OverlaySelectorView.xaml
    /// </summary>
    public partial class OverlaySelectorView : UserControl
    {
        #region Constructor
        public OverlaySelectorView()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public OverlaySelectorViewModel? ViewModel => DataContext as OverlaySelectorViewModel;

        #endregion

        private void ShowInputTraces_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadInputTracesView();
        }

        private void ShowAttitude_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadAttitudeView();
        }

        private void ShowWindDirectionCompass_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadWindDirectionCompass();
        }
    }
}

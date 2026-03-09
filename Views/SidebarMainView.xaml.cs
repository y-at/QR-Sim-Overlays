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
        public SidebarMainView()
        {
            InitializeComponent();
        }
        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            // Still unsure if I want to do open source for this project, link to github landing page for now
            try
            {
                System.Diagnostics.Process.Start("https://github.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open GitHub link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

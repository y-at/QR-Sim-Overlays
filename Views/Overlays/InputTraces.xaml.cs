using iRacing_Quick_Release.ViewModels.Overlays;
using System.Windows;
using System.Windows.Input;

namespace iRacing_Quick_Release.Views
{
    public partial class InputTraces : Window
    {
        public InputTraces(InputTracesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            this.Topmost = true;
        }
    }
}
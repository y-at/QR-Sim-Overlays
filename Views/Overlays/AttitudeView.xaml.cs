using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using QRO.ViewModels.Overlays;

namespace QRO.Views.Overlays
{
    /// <summary>
    /// Interaction logic for AttitudeView.xaml
    /// </summary>
    public partial class AttitudeView : Window
    {
        public AttitudeView(AttitudeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            // Allows overlay to be moved by mouse drag
            this.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };
            this.Topmost = true;
        }
    }
}

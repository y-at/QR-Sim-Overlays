using Prism.Mvvm;

namespace QRO.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "QRO";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}

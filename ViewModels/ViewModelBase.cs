using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iRacing_Quick_Release.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private bool _isDisposed;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool IsDisposed => _isDisposed;

        #endregion

        #region Protected Methods
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        /// <summary>
        /// Override this method in derived classes to clean up resources and event subscriptions.
        /// </summary>
        public virtual void Dispose()
        {
            // Base implementation - override in derived classes
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dispose();
            }
        }
    }
}

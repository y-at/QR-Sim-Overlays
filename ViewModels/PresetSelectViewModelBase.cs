using Prism.Regions;
using QRO.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRO.ViewModels
{
    public class PresetSelectViewModelBase : ViewModelBase
    {
        #region Fields

        private ObservableCollection<FilePresetModel> _presets = new();
        private FilePresetModel _selectedFilePreset;
        private string _newPresetName;
        private string _statusMessage;

        #endregion

        #region Public Methods

        /// <summary>
        /// Collection of saved graphics settings presets for the active mode (Monitor or VR), loaded from the QRO appdata directory.
        /// </summary>
        public ObservableCollection<FilePresetModel> Presets
        {
            get => _presets;
            private set => SetProperty(ref _presets, value);
        }

        /// <summary>
        /// The currently selected preset from the <see cref="Presets"/> collection. Used as the target for Load and Delete operations.
        /// </summary>
        public FilePresetModel SelectedFilePreset
        {
            get => _selectedFilePreset;
            set => SetProperty(ref _selectedFilePreset, value);
        }

        /// <summary>
        /// The name entered by the user for saving a new preset. Validated and used as the filename when the Save command is executed.
        /// </summary>
        public string NewPresetName
        {
            get => _newPresetName;
            set => SetProperty(ref _newPresetName, value);
        }

        /// <summary>
        /// The message presented to the user in the view to provide feedback on operations like saving, loading, and deleting presets, or validation errors.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region Protected Methods



        #endregion
    }
}

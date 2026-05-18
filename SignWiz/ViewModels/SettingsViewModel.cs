using System.ComponentModel;

namespace com.kizwiz.signwiz.ViewModels
{
    /// <summary>
    /// View model for managing application settings
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Properties
        public bool IsToggled { get; set; }
        public double Value { get; set; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Protected Methods
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

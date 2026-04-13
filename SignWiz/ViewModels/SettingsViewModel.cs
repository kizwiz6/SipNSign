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
        public bool IsTransparentFeedback
        {
            get => Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            set
            {
                Preferences.Set(Constants.TRANSPARENT_FEEDBACK_KEY, value);
                OnPropertyChanged(nameof(IsTransparentFeedback));
            }
        }
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

﻿using System.ComponentModel;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// View model for managing application settings
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Properties
        public bool IsToggled { get; set; }
        public double Value { get; set; }
        public bool IsSoberMode
        {
            get => Preferences.Get(Constants.SOBER_MODE_KEY, false);
            set
            {
                Preferences.Set(Constants.SOBER_MODE_KEY, value);
                OnPropertyChanged(nameof(IsSoberMode));
            }
        }
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

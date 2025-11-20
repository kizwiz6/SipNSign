using Android.App;
using Android.Content;
using Android.Media;
using com.kizwiz.sipnsign.Services;
using System;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Platforms.Android
{
    public class AudioFocusService : IAudioFocusService
    {
        private readonly AudioManager? _audioManager;
        private AudioManager.IOnAudioFocusChangeListener? _listener;

        public AudioFocusService()
        {
            _audioManager = Android.App.Application.Context?.GetSystemService(Context.AudioService) as AudioManager;
        }

        public bool RequestTransientDuckingFocus()
        {
            try
            {
                if (_audioManager == null) return false;

                if (_listener == null)
                    _listener = new SimpleListener();

                _audioManager.RequestAudioFocus(_listener, Android.Media.Stream.Music, AudioFocus.GainTransientMayDuck);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RequestTransientDuckingFocus failed: {ex}");
                return false;
            }
        }

        public void AbandonFocus()
        {
            try
            {
                if (_audioManager == null || _listener == null) return;
                _audioManager.AbandonAudioFocus(_listener);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AbandonFocus failed: {ex}");
            }
        }

        class SimpleListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
        {
            public void OnAudioFocusChange(AudioFocus focusChange)
            {
                // no-op; ducking handled by platform
            }
        }
    }
}
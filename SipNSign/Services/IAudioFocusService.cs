namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Platform abstraction to request transient "ducking" audio focus on Android so video playback doesn't steal/stop other audio playback or show a media notification.
    /// </summary>
    public interface IAudioFocusService
    {
        /// <summary>
        /// Request transient audio focus with "may duck" semantics. Return true if granted.
        /// </summary>
        bool RequestTransientDuckingFocus();

        /// <summary>
        /// Abandon previously requested audio focus.
        /// </summary>
        void AbandonFocus();
    }
}

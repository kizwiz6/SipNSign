namespace com.kizwiz.signwiz.Messages
{
    /// <summary>
    /// Sent when the app is going to sleep (backgrounded).
    /// </summary>
    public record AppSleepMessage;

    /// <summary>
    /// Sent when the app is resuming from sleep (foregrounded).
    /// </summary>
    public record AppResumeMessage;
}

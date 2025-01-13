namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Records user activities and progress in the application
    /// </summary
    public class ActivityLog
    {
        #region Properties
        /// <summary>
        /// Unique identifier for the activity
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Description of the activity performed
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name of the icon representing this activity
        /// </summary>
        public string IconName { get; set; }
        /// <summary>
        /// When the activity occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Score or result of the activity
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// Type of activity performed
        /// </summary>
        public ActivityType Type { get; set; }
        #endregion
    }
}

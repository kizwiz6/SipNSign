namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Represents an achievement that can be unlocked by users in the application
    /// </summary>
    public class Achievement
    {
        #region Properties
        /// <summary>
        /// Unique identifier for the achievement
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Display title of the achievement
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Detailed description of how to earn the achievement
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name of the icon file representing this achievement
        /// </summary>
        public string IconName { get; set; }
        /// <summary>
        /// Indicates if the achievement has been unlocked
        /// </summary>
        public bool IsUnlocked { get; set; }
        /// <summary>
        /// The date when the achievement was unlocked
        /// </summary>
        public DateTime? UnlockedDate { get; set; }
        /// <summary>
        /// Current progress towards achieving this achievement
        /// </summary>
        public int ProgressCurrent { get; set; }
        /// <summary>
        /// Total progress required to unlock this achievement
        /// </summary>
        public int ProgressRequired { get; set; }
        #endregion
    }
}

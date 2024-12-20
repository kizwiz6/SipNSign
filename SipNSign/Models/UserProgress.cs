namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Represents the user's progress in the application.
    /// </summary>
    public class UserProgress
    {
        /// <summary>
        /// Gets or sets the number of signs learned by the user.
        /// </summary>
        public int SignsLearned { get; set; }

        /// <summary>
        /// Gets or sets the number of signs practiced in Guess Mode.
        /// </summary>
        public int GuessModeSigns { get; set; }

        /// <summary>
        /// Gets or sets the number of signs practiced in Perform Mode.
        /// </summary>
        public int PerformModeSigns { get; set; }

        /// <summary>
        /// Gets or sets the current practice streak.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// Gets or sets the number of correct answers in a row.
        /// </summary>
        public int CorrectInARow { get; set; }

        /// <summary>
        /// Gets or sets the user's accuracy percentage.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the total number of attempts made by the user.
        /// </summary>
        public int TotalAttempts { get; set; }

        /// <summary>
        /// Gets or sets the total number of correct attempts.
        /// </summary>
        public int CorrectAttempts { get; set; }

        /// <summary>
        /// Gets or sets the total practice time spent by the user.
        /// </summary>
        public TimeSpan TotalPracticeTime { get; set; }

        /// <summary>
        /// Gets or sets the list of achievements earned by the user.
        /// </summary>
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        /// <summary>
        /// Gets or sets the list of activity logs for the user.
        /// </summary>
        public List<ActivityLog> Activities { get; set; } = new List<ActivityLog>();
    }
}

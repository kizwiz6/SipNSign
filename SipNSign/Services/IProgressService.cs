using com.kizwiz.sipnsign.Models;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Interface for managing user progress and achievements
    /// </summary>
    public interface IProgressService
    {
        #region Methods
        /// <summary>
        /// Retrieves current user progress
        /// </summary>
        Task<UserProgress> GetUserProgressAsync();
        /// <summary>
        /// Logs a new activity
        /// </summary>
        Task LogActivityAsync(ActivityLog activity);
        /// <summary>
        /// Updates achievement status
        /// </summary>
        Task UpdateAchievementsAsync();
        /// <summary>
        /// Updates user streak status
        /// </summary>
        Task<bool> UpdateStreakAsync();
        /// <summary>
        /// Saves user progress
        /// </summary>
        Task SaveProgressAsync(UserProgress progress);
        #endregion
    }
}

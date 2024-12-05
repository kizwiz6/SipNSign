using com.kizwiz.sipnsign.Models;

namespace com.kizwiz.sipnsign.Services
{
    public interface IProgressService
    {
        Task<UserProgress> GetUserProgressAsync();
        Task LogActivityAsync(ActivityLog activity);
        Task UpdateAchievementsAsync();
        Task<bool> UpdateStreakAsync();
        Task SaveProgressAsync(UserProgress progress);
    }
}

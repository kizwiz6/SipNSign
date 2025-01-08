namespace com.kizwiz.sipnsign.Models
{
    public class AchievementItem
    {
        public required string Id { get; set; }
        public required string Icon { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsUnlocked { get; set; }
        public double Progress { get; set; } // 0.0 to 1.0
        public string ProgressText { get; set; } // e.g., "5/10"
        public DateTime? UnlockedDate { get; set; }
    }
}

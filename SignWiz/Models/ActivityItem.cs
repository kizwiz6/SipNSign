namespace com.kizwiz.signwiz.Models
{
    public class ActivityItem
    {
        public required string Icon { get; set; }
        public required string Description { get; set; }
        public required string TimeAgo { get; set; }
        public required string Score { get; set; }
        /// <summary>
        /// Name of the sign associated with this activity, used for navigation to sign details.
        /// </summary>
        public string? SignName { get; set; }
    }
}

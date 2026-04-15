namespace com.kizwiz.signwiz.Models
{
    /// <summary>
    /// Represents a player with their final ranking for the game over screen
    /// </summary>
    public class RankedPlayer
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public bool IsWinner { get; set; }
    }
}

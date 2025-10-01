namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Parameters for configuring a game session
    /// </summary>
    public class GameParameters
    {
        /// <summary>
        /// Whether this is a multiplayer game
        /// </summary>
        public bool IsMultiplayer { get; set; }

        /// <summary>
        /// List of players in the game
        /// </summary>
        public List<Player> Players { get; set; } = new();

        /// <summary>
        /// Number of questions/signs to be presented in this game session
        /// </summary>
        public int QuestionsCount { get; set; } = Constants.DEFAULT_PERFORM_QUESTIONS;
    }
}
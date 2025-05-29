namespace com.kizwiz.sipnsign.Models
{
    public class GameParameters
    {
        public bool IsMultiplayer { get; set; }
        public List<Player> Players { get; set; } = new();
    }
}
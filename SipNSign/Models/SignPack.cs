using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Models
{
    public class SignPack
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required List<SignCategory> Categories { get; set; }
        public bool IsUnlocked { get; set; }
    }
}

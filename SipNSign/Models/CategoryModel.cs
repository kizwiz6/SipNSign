using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Represents a category of signs with its associated metadata
    /// </summary>
    public class CategoryModel
    {
        /// <summary>
        /// The category enumeration value
        /// </summary>
        public SignCategory Category { get; set; }

        /// <summary>
        /// User-friendly display name for the category
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Description of signs in this category
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Name of the icon file representing this category
        /// </summary>
        public string IconName { get; set; } = string.Empty;

        /// <summary>
        /// Collection of signs belonging to this category
        /// </summary>
        public List<SignModel> Signs { get; set; } = new List<SignModel>();
    }
}

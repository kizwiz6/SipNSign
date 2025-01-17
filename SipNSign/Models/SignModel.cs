using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Models
{
    /// <summary>
    /// Represents a sign in the SipNSign game.
    /// </summary>
    public class SignModel
    {
        /// <summary>
        /// Gets or sets the path to the sign's image.
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the path to the sign's video.
        /// </summary>
        public string VideoPath { get; set; }

        /// <summary>
        /// Gets or sets the correct answer for the sign (e.g., "Hello").
        /// </summary>
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// Gets or sets the list of multiple choice options for the user to select from.
        /// </summary>
        public List<string> Choices { get; set; }

        /// <summary>
        /// Gets or sets the specific sign language (e.g., BSL or ASL).
        /// </summary>
        public SignLanguage Language { get; set; }
        /// <summary>
        /// Gets or sets the category for the sign (e.g., "Numbers", "Animals").
        /// </summary>
        public SignCategory Category { get; set; }
        /// <summary>
        /// Indicates if this sign is part of premium content
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// The set/pack this sign belongs to (e.g., "Base", "Premium Pack 1", etc.)
        /// </summary>
        public string PackName { get; set; }
    }
}

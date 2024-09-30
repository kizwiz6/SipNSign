using System;
using System.Collections.Generic;

namespace SipNSign.Models
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
        /// Gets or sets the correct answer for the sign (e.g., "Hello").
        /// </summary>
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// Gets or sets the list of multiple choice options for the user to select from.
        /// </summary>
        public List<string> Choices { get; set; }
    }
}

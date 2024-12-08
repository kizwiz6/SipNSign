using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign
{
    public static class Constants
    {
        // Theme settings
        public const string THEME_KEY = "app_theme";
        public const string FONT_SIZE_KEY = "font_size";
        public const string DIFFICULTY_KEY = "difficulty_level";
        public const string TRANSLATIONS_KEY = "show_translations";
        public const string VIDEO_SPEED_KEY = "video_speed";
        public const string CONTRAST_KEY = "high_contrast";
        public const string HAND_DOMINANCE_KEY = "hand_dominance";
        public const string OFFLINE_MODE_KEY = "offline_mode";

        // Feedback delay settings
        public const string INCORRECT_DELAY_KEY = "IncorrectAnswerDelay";
        public const int DEFAULT_DELAY = 5000;
        public const int MIN_DELAY = 2000;
        public const int MAX_DELAY = 10000;
    }
}

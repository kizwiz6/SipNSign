namespace com.kizwiz.sipnsign
{
    public static class Constants
    {
        // Theme settings
        public const string THEME_KEY = "app_theme";
        public const string SOBER_MODE_KEY = "sober_mode";

        // Timer settings
        public const string TIMER_DURATION_KEY = "TimerDuration";
        public const int DEFAULT_TIMER_DURATION = 10;
        public const int MIN_TIMER_DURATION = 5;
        public const int MAX_TIMER_DURATION = 15;

        // Feedback delay settings
        public const string SHOW_FEEDBACK_KEY = "show_feedback";
        public const string INCORRECT_DELAY_KEY = "IncorrectAnswerDelay";
        public const int DEFAULT_DELAY = 5000;
        public const int MIN_DELAY = 2000;
        public const int MAX_DELAY = 10000;
        public const string TRANSPARENT_FEEDBACK_KEY = "TransparentFeedback";

        // Guess Mode questions limit
        public const string GUESS_MODE_QUESTIONS_KEY = "GuessQuestions";
        public const int MIN_QUESTIONS = 10;
        public const int MAX_QUESTIONS = 100;
        public const int DEFAULT_QUESTIONS = 20;
    }
}

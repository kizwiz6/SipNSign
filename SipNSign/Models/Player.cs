using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Models
{
    public partial class Player : ObservableObject
    {
        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private bool _isMainPlayer = false;

        // Enhanced to ensure it raises property changed events
        private int _score = 0;
        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    Debug.WriteLine($"Player {Name}: Score changed from {_score} to {value}");
                    _score = value;
                    OnPropertyChanged(nameof(Score)); // Explicit notification
                }
            }
        }

        // Track if player has answered (regardless of correctness)
        private bool _hasAnswered = false;
        public bool HasAnswered
        {
            get => _hasAnswered;
            set
            {
                if (_hasAnswered != value)
                {
                    Debug.WriteLine($"Player {Name}: HasAnswered changed from {_hasAnswered} to {value}");
                    _hasAnswered = value;
                    OnPropertyChanged(nameof(HasAnswered));
                }
            }
        }

        // Enhanced to ensure it raises property changed events
        private bool _gotCurrentAnswerCorrect = false;
        public bool GotCurrentAnswerCorrect
        {
            get => _gotCurrentAnswerCorrect;
            set
            {
                if (_gotCurrentAnswerCorrect != value)
                {
                    Debug.WriteLine($"Player {Name}: GotCurrentAnswerCorrect changed from {_gotCurrentAnswerCorrect} to {value}");
                    _gotCurrentAnswerCorrect = value;
                    OnPropertyChanged(nameof(GotCurrentAnswerCorrect));
                    OnPropertyChanged(nameof(AnswerStatus));
                    OnPropertyChanged(nameof(IndicatorColor));
                }
            }
        }

        public string AnswerStatus => HasAnswered ?
            (GotCurrentAnswerCorrect ? "Correct! ✓" : "Incorrect ✗") :
            "Not answered";

        // Color for the indicator circle
        public Color IndicatorColor
        {
            get
            {
                if (!HasAnswered)
                    return Colors.Gray; // Not answered yet

                // Use theme-aware colors that work across different themes
                return GotCurrentAnswerCorrect ?
                    Color.FromArgb("#22C55E") : // Green-500 (good contrast on most backgrounds)
                    Color.FromArgb("#EF4444");   // Red-500 (good contrast on most backgrounds)
            }
        }

        // Helper method to record an answer
        public void RecordAnswer(bool isCorrect)
        {
            HasAnswered = true;
            GotCurrentAnswerCorrect = isCorrect;

            if (isCorrect)
            {
                Score++;
            }
        }

        // Helper method to reset for new sign
        public void ResetForNewSign()
        {
            HasAnswered = false;
            GotCurrentAnswerCorrect = false;
        }
    }
}
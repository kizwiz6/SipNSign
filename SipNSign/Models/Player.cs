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
                    OnPropertyChanged(nameof(Score));
                }
            }
        }

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

        public Color IndicatorColor
        {
            get
            {
                if (!HasAnswered)
                    return Colors.Gray;

                return GotCurrentAnswerCorrect ?
                    Color.FromArgb("#22C55E") :
                    Color.FromArgb("#EF4444");
            }
        }

        /// <summary>
        /// Records or updates an answer, handling score adjustments for answer changes
        /// </summary>
        public void RecordAnswer(bool isCorrect)
        {
            // If player already answered, we need to adjust score based on change
            if (HasAnswered)
            {
                // If they had it correct before and now incorrect, subtract
                if (GotCurrentAnswerCorrect && !isCorrect)
                {
                    Score--;
                    Debug.WriteLine($"Player {Name}: Changed answer from correct to incorrect, score decreased");
                }
                // If they had it incorrect before and now correct, add
                else if (!GotCurrentAnswerCorrect && isCorrect)
                {
                    Score++;
                    Debug.WriteLine($"Player {Name}: Changed answer from incorrect to correct, score increased");
                }
                // If same answer (correct->correct or incorrect->incorrect), no change
                else
                {
                    Debug.WriteLine($"Player {Name}: Same answer selected, no score change");
                }
            }
            else
            {
                // First time answering - only add score if correct
                if (isCorrect)
                {
                    Score++;
                    Debug.WriteLine($"Player {Name}: First answer is correct, score increased");
                }
                else
                {
                    Debug.WriteLine($"Player {Name}: First answer is incorrect, no score change");
                }
            }

            // Update the answer state
            HasAnswered = true;
            GotCurrentAnswerCorrect = isCorrect;
        }

        /// <summary>
        /// Resets player state for a new sign
        /// </summary>
        public void ResetForNewSign()
        {
            HasAnswered = false;
            GotCurrentAnswerCorrect = false;

            OnPropertyChanged(nameof(AnswerStatus));
            OnPropertyChanged(nameof(IndicatorColor));

            Debug.WriteLine($"Player {Name}: Reset for new sign");
        }
    }
}
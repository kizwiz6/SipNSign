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

        // Make SelectedAnswer observable so UI triggers update for DataTriggers
        private int _selectedAnswer = 0;
        public int SelectedAnswer
        {
            get => _selectedAnswer;
            set
            {
                if (_selectedAnswer != value)
                {
                    _selectedAnswer = value;
                    OnPropertyChanged(nameof(SelectedAnswer));
                }
            }
        }

        // NEW: For Guess mode - store which answer they selected (1-4)
        private int? _selectedAnswerNumber = null;
        public int? SelectedAnswerNumber
        {
            get => _selectedAnswerNumber;
            set
            {
                if (_selectedAnswerNumber != value)
                {
                    _selectedAnswerNumber = value;
                    OnPropertyChanged(nameof(SelectedAnswerNumber));
                    OnPropertyChanged(nameof(AnswerDisplayText));
                }
            }
        }

        // NEW: For Guess mode - what their selected answer text was
        private string? _selectedAnswerText = null;
        public string? SelectedAnswerText
        {
            get => _selectedAnswerText;
            set
            {
                if (_selectedAnswerText != value)
                {
                    _selectedAnswerText = value;
                    OnPropertyChanged(nameof(SelectedAnswerText));
                }
            }
        }

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
                    OnPropertyChanged(nameof(AnswerStatus));
                    OnPropertyChanged(nameof(IndicatorColor));
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

        // NEW: Display text showing their answer choice (for Guess mode)
        public string AnswerDisplayText
        {
            get
            {
                if (!HasAnswered)
                    return "Waiting...";

                if (SelectedAnswerNumber.HasValue)
                {
                    return $"Selected: {SelectedAnswerNumber}";
                }

                return GotCurrentAnswerCorrect ? "Correct! ✓" : "Incorrect ✗";
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
        /// Records a guess mode answer with the answer number (1-4) and text
        /// NOTE: Do NOT change Score here — scoring happens only on Confirm.
        /// This allows players to change their selection before confirming.
        /// </summary>
        public void RecordGuessAnswer(int answerNumber, string answerText, bool isCorrect)
        {
            SelectedAnswerNumber = answerNumber;
            SelectedAnswerText = answerText;

            // Keep SelectedAnswer in sync for XAML triggers (observable update above)
            SelectedAnswer = answerNumber;

            HasAnswered = true;
            GotCurrentAnswerCorrect = isCorrect;

            // DON'T update Score here - scoring must be applied when the round is confirmed
            OnPropertyChanged(nameof(AnswerDisplayText));
            Debug.WriteLine($"Player {Name}: Selected answer {answerNumber} ({answerText}) - {(isCorrect ? "Correct" : "Incorrect")}");
        }

        /// <summary>
        /// Records or updates an answer, handling score adjustments for answer changes
        /// (Used for score-confirmation UI actions that explicitly adjust scores.)
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

            // Explicitly trigger indicator update
            OnPropertyChanged(nameof(HasAnswered));
            OnPropertyChanged(nameof(GotCurrentAnswerCorrect));
            OnPropertyChanged(nameof(IndicatorColor));
            OnPropertyChanged(nameof(AnswerStatus));
            OnPropertyChanged(nameof(AnswerDisplayText));
        }

        /// <summary>
        /// Resets player state for a new sign
        /// </summary>
        public void ResetForNewSign()
        {
            HasAnswered = false;
            GotCurrentAnswerCorrect = false;
            SelectedAnswerNumber = null;
            SelectedAnswerText = null;
            SelectedAnswer = 0;

            OnPropertyChanged(nameof(AnswerStatus));
            OnPropertyChanged(nameof(IndicatorColor));
            OnPropertyChanged(nameof(AnswerDisplayText));

            Debug.WriteLine($"Player {Name}: Reset for new sign");
        }
    }
}
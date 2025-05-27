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
                    OnPropertyChanged(nameof(GotCurrentAnswerCorrect)); // Explicit notification
                    OnPropertyChanged(nameof(AnswerStatus)); // Update dependent property
                }
            }
        }

        public string AnswerStatus => GotCurrentAnswerCorrect ? "Correct! ✓" : "Incorrect ✗";
    }
}
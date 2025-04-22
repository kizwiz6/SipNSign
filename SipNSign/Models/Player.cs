using CommunityToolkit.Mvvm.ComponentModel;

namespace com.kizwiz.sipnsign.Models
{
    public partial class Player : ObservableObject
    {
        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private bool _isMainPlayer = false;

        [ObservableProperty]
        private int _score = 0;

        [ObservableProperty]
        private bool _gotCurrentAnswerCorrect = false;
    }
}

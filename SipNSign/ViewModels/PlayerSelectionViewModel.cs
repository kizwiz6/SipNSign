using com.kizwiz.sipnsign.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace com.kizwiz.sipnsign.ViewModels
{
    public partial class PlayerSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _mainPlayerName = "You";

        [ObservableProperty]
        private ObservableCollection<Player> _additionalPlayers = new();

        public ObservableCollection<Player> Players => new ObservableCollection<Player>(
            new[] { new Player { Name = MainPlayerName, IsMainPlayer = true } }
            .Concat(AdditionalPlayers));

        [RelayCommand]
        private void AddPlayer()
        {
            AdditionalPlayers.Add(new Player { Name = $"Player {AdditionalPlayers.Count + 2}" });
        }

        [RelayCommand]
        private void RemovePlayer(Player player)
        {
            if (!player.IsMainPlayer)
            {
                AdditionalPlayers.Remove(player);
            }
        }

        // Update Players when properties change
        partial void OnMainPlayerNameChanged(string value) => OnPropertyChanged(nameof(Players));
        partial void OnAdditionalPlayersChanged(ObservableCollection<Player> value) => OnPropertyChanged(nameof(Players));
    }
}

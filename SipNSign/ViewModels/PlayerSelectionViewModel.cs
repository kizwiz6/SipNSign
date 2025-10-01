using com.kizwiz.sipnsign.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace com.kizwiz.sipnsign.ViewModels
{
    public partial class PlayerSelectionViewModel : ObservableObject
    {
        private const int MAX_PLAYERS = 10;

        [ObservableProperty]
        private string _mainPlayerName = "You";

        [ObservableProperty]
        private ObservableCollection<Player> _additionalPlayers = new();

        // Property to check if we can add more players
        public bool CanAddMorePlayers => (AdditionalPlayers.Count + 1) < MAX_PLAYERS;

        // Property to show remaining player slots
        public string PlayerCountText => $"{AdditionalPlayers.Count + 1}/{MAX_PLAYERS} Players";

        public ObservableCollection<Player> Players => new ObservableCollection<Player>(
            new[] { new Player { Name = MainPlayerName, IsMainPlayer = true } }
            .Concat(AdditionalPlayers));

        [RelayCommand(CanExecute = nameof(CanAddMorePlayers))]
        private void AddPlayer()
        {
            if (!CanAddMorePlayers)
            {
                // This shouldn't happen due to CanExecute, but defensive check
                return;
            }

            AdditionalPlayers.Add(new Player { Name = $"Player {AdditionalPlayers.Count + 2}" });

            // Update command state
            AddPlayerCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanAddMorePlayers));
            OnPropertyChanged(nameof(PlayerCountText));
        }

        [RelayCommand]
        private void RemovePlayer(Player player)
        {
            if (!player.IsMainPlayer)
            {
                AdditionalPlayers.Remove(player);

                // Update command state after removal
                AddPlayerCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanAddMorePlayers));
                OnPropertyChanged(nameof(PlayerCountText));
            }
        }

        // Update Players when properties change
        partial void OnMainPlayerNameChanged(string value)
        {
            OnPropertyChanged(nameof(Players));
        }

        partial void OnAdditionalPlayersChanged(ObservableCollection<Player> value)
        {
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(CanAddMorePlayers));
            OnPropertyChanged(nameof(PlayerCountText));
            AddPlayerCommand.NotifyCanExecuteChanged();
        }
    }
}
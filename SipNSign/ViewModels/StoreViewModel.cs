using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using com.kizwiz.sipnsign.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    public partial class StoreViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IIAPService _iapService;
        private readonly ILoggingService _logger;
        private readonly Dictionary<string, bool> _purchasedPacks = new();

        [ObservableProperty]
        private bool _isLoading;

        public ICommand PurchasePackCommand { get; }
        public ICommand RestorePurchasesCommand { get; }

        public StoreViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _iapService = serviceProvider.GetRequiredService<IIAPService>();
            _logger = serviceProvider.GetRequiredService<ILoggingService>();
            PurchasePackCommand = new AsyncRelayCommand<string>(PurchasePack);
            RestorePurchasesCommand = new AsyncRelayCommand(RestorePurchases);

            // Initial empty state, will be populated in InitializeAsync
            _purchasedPacks["starter"] = true;
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadPurchasedPacks();
                await _iapService.InitializeAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPurchasedPacks()
        {
            try
            {
                _purchasedPacks.Clear();
                _purchasedPacks["starter"] = true;  // Starter pack always available

                foreach (var packId in new[] { "animals", "geography", "food_drink", "emotions" })
                {
                    bool isPurchased = await _iapService.IsProductPurchasedAsync(packId);
                    _purchasedPacks[packId] = isPurchased;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading purchased packs: {ex.Message}");
            }
        }

        private async Task PurchasePack(string? packId)
        {
            if (string.IsNullOrEmpty(packId)) return;

            try
            {
                IsLoading = true;
                bool success = await _iapService.PurchaseAsync(packId);

                if (success)
                {
                    _purchasedPacks[packId] = true;
                    await Application.Current.MainPage.DisplayAlert("Success",
                        $"Successfully purchased the {GetPackName(packId)} pack!", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error purchasing pack: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Failed to complete purchase. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RestorePurchases()
        {
            try
            {
                IsLoading = true;
                bool success = await _iapService.RestorePurchasesAsync();

                if (success)
                {
                    await LoadPurchasedPacks();  // Refresh the purchased packs
                    await Application.Current.MainPage.DisplayAlert("Success",
                        "Your purchases have been restored!", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Failed to restore purchases. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error restoring purchases: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Failed to restore purchases. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetPackName(string packId) => packId switch
        {
            "animals" => "Animals",
            "geography" => "Geography",
            "food_drink" => "Food & Drink",
            "emotions" => "Emotions",
            _ => "Unknown"
        };
    }
}
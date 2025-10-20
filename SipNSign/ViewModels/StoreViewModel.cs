using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using com.kizwiz.sipnsign.Services;
using System.Diagnostics;
using System.Globalization;
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

        [ObservableProperty]
        private bool _isAnimalPackPurchasable = true;

        [ObservableProperty]
        private bool _isGeographyPackPurchasable = true;

        [ObservableProperty]
        private bool _isFoodPackPurchasable = true;

        [ObservableProperty]
        private bool _isEmotionsPackPurchasable = true;

        [ObservableProperty]
        private bool _isSportsHobbiesPackPurchasable = true;

        [ObservableProperty]
        private bool _isThemesPackPurchasable = true;

        [ObservableProperty]
        private string _animalPackButtonText;

        [ObservableProperty]
        private string _geographyPackButtonText;

        [ObservableProperty]
        private string _foodPackButtonText;

        [ObservableProperty]
        private string _emotionsPackButtonText;

        [ObservableProperty]
        private string _sportsHobbiesPackButtonText;

        [ObservableProperty]
        private string _themesPackButtonText;

        private readonly Dictionary<string, Dictionary<string, decimal>> _regionalPrices = new()
        {
            {
                "US", new()
                {
                    { "animals", 0.99m },
                    { "geography", 0.99m },
                    { "food_drink", 0.99m },
                    { "emotions", 0.99m },
                    { "sports_hobbies", 0.99m },
                    { "premium_themes", 0.99m }
                }
            },
            {
                "GB", new()
                {
                    { "animals", 0.99m },
                    { "geography", 0.99m },
                    { "food_drink", 0.99m },
                    { "emotions", 0.99m },
                    { "sports_hobbies", 0.99m },
                    { "premium_themes", 0.99m }
                }
            },
            {
                "EU", new()
                {
                    { "animals", 0.89m },
                    { "geography", 0.89m },
                    { "food_drink", 0.89m },
                    { "emotions", 0.89m },
                    { "sports_hobbies", 0.99m },
                    { "premium_themes", 0.89m }
                }
            },
            {
                "JP", new()
                {
                    { "animals", 120m },
                    { "geography", 120m },
                    { "food_drink", 120m },
                    { "emotions", 120m },
                    { "sports_hobbies", 0.99m },
                    { "premium_themes", 120m }
                }
            },
            {
                "AU", new()
                {
                    { "animals", 1.49m },
                    { "geography", 1.49m },
                    { "food_drink", 1.49m },
                    { "emotions", 1.49m },
                    { "sports_hobbies", 0.99m },
                    { "premium_themes", 1.49m }
                }
            }
        };

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
                await UpdateButtonStates();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string FormatPrice(string packId)
        {
            try
            {
                var currentRegion = GetCurrentRegion();
                var regionPrices = _regionalPrices[currentRegion];
                var price = regionPrices[packId];

                // Get the appropriate culture info based on the region
                var culture = currentRegion switch
                {
                    "US" => new CultureInfo("en-US"),
                    "GB" => new CultureInfo("en-GB"),
                    "EU" => new CultureInfo("es-ES"),
                    "JP" => new CultureInfo("ja-JP"),
                    "AU" => new CultureInfo("en-AU"),
                    _ => CultureInfo.CurrentCulture
                };

                return price.ToString("C", culture);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error formatting price: {ex.Message}");
                return "Error";
            }
        }

        private string GetCurrentRegion()
        {
            var regionInfo = RegionInfo.CurrentRegion;

            return regionInfo.TwoLetterISORegionName switch
            {
                "US" => "US",
                "GB" => "GB",
                "DE" or "FR" or "IT" or "ES" or "NL" or "BE" or "PT" or "IE" or "AT" or "FI"
                    or "GR" or "SK" or "SI" or "LV" or "LT" or "EE" or "CY" or "MT" => "EU",
                "JP" => "JP",
                "AU" => "AU",
                _ => "GB"
            };
        }

        private async Task UpdateButtonStates()
        {
            foreach (var packId in new[] { "animals", "geography", "food_drink", "emotions", "sports_hobbies", "premium_themes" })
            {
                bool isPurchased = await _iapService.IsProductPurchasedAsync(packId);
                var priceText = isPurchased ? "OWNED" : FormatPrice(packId);

                switch (packId)
                {
                    case "animals":
                        AnimalPackButtonText = priceText;
                        IsAnimalPackPurchasable = !isPurchased;
                        break;
                    case "geography":
                        GeographyPackButtonText = priceText;
                        IsGeographyPackPurchasable = !isPurchased;
                        break;
                    case "food_drink":
                        FoodPackButtonText = priceText;
                        IsFoodPackPurchasable = !isPurchased;
                        break;
                    case "emotions":
                        EmotionsPackButtonText = priceText;
                        IsEmotionsPackPurchasable = !isPurchased;
                        break;
                    case "sports_hobbies":
                        SportsHobbiesPackButtonText = priceText;
                        IsSportsHobbiesPackPurchasable = !isPurchased;
                        break;
                    case "premium_themes":
                        ThemesPackButtonText = priceText;
                        IsThemesPackPurchasable = !isPurchased;
                        break;
                }
            }
        }

        private async Task LoadPurchasedPacks()
        {
            try
            {
                _purchasedPacks.Clear();
                _purchasedPacks["starter"] = true;

                foreach (var packId in new[] { "animals", "geography", "food_drink", "emotions", "sports_hobbies", "premium_themes" })
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
                    await UpdateButtonStates();
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
                    await LoadPurchasedPacks();
                    await UpdateButtonStates();
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
            "sports_hobbies" => "Sports & Hobbies",
            "premium_themes" => "Premium Themes",
            _ => "Unknown"
        };
    }
}
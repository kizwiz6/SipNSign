namespace com.kizwiz.sipnsign.Services
{
    public class IAPService : IIAPService
    {
        private readonly ILoggingService _logger;
        private const string PURCHASED_PACKS_KEY = "purchased_packs";
        private readonly Dictionary<string, decimal> _productPrices;

        public IAPService(ILoggingService logger)
        {
            _logger = logger;
            _productPrices = new Dictionary<string, decimal>
            {
                { "animals", 0.99m },
                { "geography", 0.99m },
                { "food_drink", 0.99m },
                { "emotions", 0.99m },
                { "sports_hobbies", 0.99m }
            };
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.Debug("Initializing IAP service");
                // Here you would initialize your actual payment provider
                await Task.Delay(100); // Simulate initialization
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing IAP service: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PurchaseAsync(string productId)
        {
            try
            {
                _logger.Debug($"Starting purchase for product: {productId}");

                // Here you would implement actual payment processing
                bool purchaseConfirmed = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Purchase",
                    $"Would you like to purchase this pack for £{_productPrices[productId]:F2}?",
                    "Yes", "No");

                if (purchaseConfirmed)
                {
                    var purchasedPacks = GetPurchasedPacks();
                    if (!purchasedPacks.Contains(productId))
                    {
                        purchasedPacks.Add(productId);
                        SavePurchasedPacks(purchasedPacks);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing purchase: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RestorePurchasesAsync()
        {
            try
            {
                _logger.Debug("Restoring purchases");
                // Here you would implement actual purchase restoration
                await Task.Delay(100); // Simulate restoration
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error restoring purchases: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsProductPurchasedAsync(string productId)
        {
            try
            {
                var purchasedPacks = GetPurchasedPacks();
                return purchasedPacks.Contains(productId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking product purchase status: {ex.Message}");
                return false;
            }
        }

        public Task<Dictionary<string, decimal>> GetProductPricesAsync()
        {
            return Task.FromResult(_productPrices);
        }

        private List<string> GetPurchasedPacks()
        {
            var json = Preferences.Get(PURCHASED_PACKS_KEY, "[]");
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        private void SavePurchasedPacks(List<string> packs)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(packs);
            Preferences.Set(PURCHASED_PACKS_KEY, json);
        }
    }
}

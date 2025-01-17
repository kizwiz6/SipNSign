namespace com.kizwiz.sipnsign.Services
{
    public interface IIAPService
    {
        Task<bool> InitializeAsync();
        Task<bool> PurchaseAsync(string productId);
        Task<bool> RestorePurchasesAsync();
        Task<bool> IsProductPurchasedAsync(string productId);
        Task<Dictionary<string, decimal>> GetProductPricesAsync();
    }
}

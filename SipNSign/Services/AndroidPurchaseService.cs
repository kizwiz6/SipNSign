using System.Diagnostics;

namespace com.kizwiz.sipnsign.Services
{
    public class AndroidPurchaseService : IPurchaseService
    {
        private readonly IInAppBilling _billing;

        public Task<IList<SignPackInfo>> GetAvailablePacks()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPremiumUnlocked()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PurchasePremiumPack(string packId)
        {
            try
            {
                var connected = await _billing.ConnectAsync();
                if (!connected) return false;

                var purchase = await _billing.PurchaseAsync(packId, ItemType.InAppPurchase);
                return purchase?.State == PurchaseState.Purchased;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Purchase failed: {ex.Message}");
                return false;
            }
        }

        public Task<bool> RestorePurchases()
        {
            throw new NotImplementedException();
        }
    }
}
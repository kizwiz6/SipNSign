namespace com.kizwiz.sipnsign.Services
{
    public interface IPurchaseService
    {
        Task<bool> IsPremiumUnlocked();
        Task<bool> PurchasePremiumPack(string packId);
        Task<IList<SignPackInfo>> GetAvailablePacks();
        Task<bool> RestorePurchases();
    }
}

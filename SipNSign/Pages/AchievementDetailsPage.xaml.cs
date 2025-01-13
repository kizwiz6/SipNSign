using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class AchievementDetailsPage : ContentPage
    {
        private readonly AchievementDetailsViewModel _viewModel;

        public AchievementDetailsPage(Achievement achievement)
        {
            try
            {
                var logger = Application.Current?.Handler?.MauiContext?.Services?.GetService<ILoggingService>();
                logger?.Debug($"Starting AchievementDetailsPage initialization for: {achievement?.Title ?? "null"}");

                // Check if Application.Current.Resources contains our converter
                logger?.Debug($"Checking for InverseBoolConverter in resources: {Application.Current?.Resources.ContainsKey("InverseBoolConverter")}");

                InitializeComponent();
                logger?.Debug("InitializeComponent completed");

                var services = Application.Current?.Handler?.MauiContext?.Services;
                logger?.Debug($"Services available: {services != null}");

                if (services == null)
                {
                    logger?.Error("Services not available - attempting fallback");
                    // Create a fallback ShareService
                    _viewModel = new AchievementDetailsViewModel(achievement, new ShareService());
                }
                else
                {
                    var shareService = services.GetService<IShareService>();
                    if (shareService == null)
                    {
                        logger?.Error("IShareService not found - using fallback");
                        shareService = new ShareService();
                    }
                    _viewModel = new AchievementDetailsViewModel(achievement, shareService);
                }

                BindingContext = _viewModel;
                logger?.Debug("AchievementDetailsPage created successfully");
            }
            catch (Exception ex)
            {
                var logger = Application.Current?.Handler?.MauiContext?.Services?.GetService<ILoggingService>();
                logger?.Error($"Error in AchievementDetailsPage constructor: {ex.Message}");
                logger?.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
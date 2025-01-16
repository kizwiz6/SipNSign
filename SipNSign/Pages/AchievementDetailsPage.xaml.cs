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

                InitializeComponent();
                logger?.Debug("InitializeComponent completed");

                var services = Application.Current?.Handler?.MauiContext?.Services;
                logger?.Debug($"Services available: {services != null}");

                if (services == null)
                {
                    logger?.Error("Services not available - attempting fallback");
                    var shareService = new ShareService();
                    var progressService = services.GetRequiredService<IProgressService>();
                    _viewModel = new AchievementDetailsViewModel(achievement, shareService, progressService);
                }
                else
                {
                    var shareService = services.GetService<IShareService>();
                    var progressService = services.GetRequiredService<IProgressService>();
                    if (shareService == null)
                    {
                        logger?.Error("IShareService not found - using fallback");
                        shareService = new ShareService();
                    }
                    _viewModel = new AchievementDetailsViewModel(achievement, shareService, progressService);
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
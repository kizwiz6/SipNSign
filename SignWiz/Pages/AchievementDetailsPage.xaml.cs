using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using com.kizwiz.signwiz.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.signwiz.Pages
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

                // Wire up the card capture delegate for image sharing
                _viewModel.CaptureCardAsync = CaptureAchievementCardAsync;

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

        /// <summary>
        /// Captures the achievement card Border as a PNG image stream.
        /// </summary>
        private async Task<Stream?> CaptureAchievementCardAsync()
        {
            try
            {
                var result = await AchievementCard.CaptureAsync();
                if (result != null)
                {
                    return await result.OpenReadAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error capturing achievement card: {ex.Message}");
            }
            return null;
        }
    }
}
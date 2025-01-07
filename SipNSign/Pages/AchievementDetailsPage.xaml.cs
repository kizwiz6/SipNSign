using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class AchievementDetailsPage : ContentPage
    {
        private AchievementDetailsViewModel _viewModel;

        public AchievementDetailsPage(Achievement achievement)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            _viewModel = new AchievementDetailsViewModel(
                Navigation,
                services.GetRequiredService<IProgressService>(),
                services.GetRequiredService<IShareService>(),
                achievement
            );

            InitializeComponent();
            BindingContext = _viewModel;
        }
    }
}
namespace com.kizwiz.sipnsign.Services
{
    public class ThemeChangeBehavior : Behavior<ContentPage>
    {
        protected override void OnAttachedTo(ContentPage page)
        {
            base.OnAttachedTo(page);
            var themeService = Application.Current?.Handler?.MauiContext?.Services.GetService<IThemeService>();
            if (themeService != null)
            {
                themeService.ThemeChanged += OnThemeChanged;
            }
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.MainPage?.Handler != null)
                {
                    // Force handler reload
                    var handler = Application.Current.MainPage.Handler;
                    Application.Current.MainPage.Handler = null;
                    Application.Current.MainPage.Handler = handler;
                }
            });
        }

        protected override void OnDetachingFrom(ContentPage page)
        {
            var themeService = Application.Current?.Handler?.MauiContext?.Services.GetService<IThemeService>();
            if (themeService != null)
            {
                themeService.ThemeChanged -= OnThemeChanged;
            }
            base.OnDetachingFrom(page);
        }
    }
}

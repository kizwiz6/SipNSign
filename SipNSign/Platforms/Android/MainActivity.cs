using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using Microsoft.Maui.Controls;

namespace com.kizwiz.sipnsign;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize |
                                ConfigChanges.Orientation |
                                ConfigChanges.UiMode |
                                ConfigChanges.ScreenLayout |
                                ConfigChanges.SmallestScreenSize |
                                ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1a237e"));

        // Register back callback
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            OnBackPressedDispatcher.AddCallback(this, new BackCallback(this));
        }
    }

    private class BackCallback : OnBackPressedCallback
    {
        private readonly MainActivity _activity;

        public BackCallback(MainActivity activity) : base(true)
        {
            _activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.Navigation.PopAsync();
                });
            }
            else
            {
                if (!Enabled) return;
                Enabled = false;
                _activity.OnBackPressedDispatcher.OnBackPressed();
                Enabled = true;
            }
        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Activity;
using com.kizwiz.sipnsign.Platforms.Android;
using System;

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
        try
        {
            base.OnCreate(savedInstanceState);

            // Set custom status bar color
            Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1a237e"));

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                OnBackPressedDispatcher.AddCallback(this, new BackCallback(this));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainActivity.OnCreate: {ex.Message}");
            throw;
        }
    }

    internal static bool IsMediaSessionFinalizerCrash(Exception? ex)
    {
        if (ex is ObjectDisposedException ode)
        {
            if (ode.ObjectName?.Contains("MediaSessionCompat", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (ode.StackTrace?.Contains("MediaControlsService.Dispose", StringComparison.Ordinal) == true)
                return true;
        }
        return false;
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

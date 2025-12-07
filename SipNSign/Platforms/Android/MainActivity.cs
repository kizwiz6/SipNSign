using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using System.Diagnostics;
using com.kizwiz.sipnsign.Platforms.Android;

namespace com.kizwiz.sipnsign;

[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize |
                                ConfigChanges.Orientation |
                                ConfigChanges.UiMode |
                                ConfigChanges.ScreenLayout |
                                ConfigChanges.SmallestScreenSize |
                                ConfigChanges.Density)]
/// <summary>
/// Main Android activity for the application
/// </summary>
public class MainActivity : MauiAppCompatActivity
{
    #region Lifecycle Methods
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try
        {
            // Ensure runtime shim registration runs BEFORE MAUI initializes/native libraries load
            MediaManagerNativeRegistration.Register();

            System.Diagnostics.Debug.WriteLine("MainActivity OnCreate starting");
            base.OnCreate(savedInstanceState);

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
    #endregion

    #region Inner Classes
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
    #endregion
}
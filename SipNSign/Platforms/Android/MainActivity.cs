using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using System.Diagnostics;

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
#if ANDROID
            Android.Util.Log.Debug("SipNSignApp", "MainActivity OnCreate starting");
#endif
            System.Diagnostics.Debug.WriteLine("MainActivity OnCreate starting");

            base.OnCreate(savedInstanceState);
            Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1a237e"));

            System.Diagnostics.Debug.WriteLine("MainActivity base.OnCreate completed");

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                System.Diagnostics.Debug.WriteLine("Registering back callback");
                OnBackPressedDispatcher.AddCallback(this, new BackCallback(this));
            }

            System.Diagnostics.Debug.WriteLine("MainActivity OnCreate completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainActivity.OnCreate: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
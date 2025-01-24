using com.kizwiz.sipnsign.Services;
using System.Diagnostics;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace com.kizwiz.sipnsign
{
    /// <summary>
    /// The main application class that initializes the app, manages themes,
    /// and sets the main page for the application.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// This constructor retrieves the user's saved theme preference,
        /// sets the initial application theme, and configures dependency injection.
        /// </summary>
        public App(IServiceProvider serviceProvider)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "==== App Constructor Start ====");
#endif
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Service provider assigned");
#endif

                InitializeComponent();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "InitializeComponent completed");
#endif

                var themeService = serviceProvider.GetService<IThemeService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", $"Theme service obtained: {themeService != null}");
#endif

                if (themeService != null)
                {
#if ANDROID
                    Android.Util.Log.Debug("SipNSignApp", "Getting current theme");
#endif
                    var savedTheme = themeService.GetCurrentTheme();
#if ANDROID
                    Android.Util.Log.Debug("SipNSignApp", $"Current theme: {savedTheme}");
                    Android.Util.Log.Debug("SipNSignApp", "Setting theme");
#endif
                    themeService.SetTheme(savedTheme);
#if ANDROID
                    Android.Util.Log.Debug("SipNSignApp", "Theme set successfully");
#endif
                }

#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Creating main page");
#endif
                var appShell = serviceProvider.GetRequiredService<AppShell>();
                MainPage = appShell;
#if ANDROID  
                Android.Util.Log.Debug("SipNSignApp", "Main page set via MainPage property");
                Android.Util.Log.Debug("SipNSignApp", "==== App Constructor End ====");
#endif
            }
            catch (Exception ex)
            {
#if ANDROID
                Android.Util.Log.Error("SipNSignApp", $"CRITICAL ERROR in App constructor: {ex.Message}");
                Android.Util.Log.Error("SipNSignApp", $"Stack trace: {ex.StackTrace}");
#endif
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
#if ANDROID
            Android.Util.Log.Debug("SipNSignApp", "Creating window");
#endif
            try
            {
                Window window = new Window(MainPage)
                {
                    Title = "SipNSign"
                };
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Window created successfully");
#endif
                return window;
            }
            catch (Exception ex)
            {
#if ANDROID
                Android.Util.Log.Error("SipNSignApp", $"Error creating window: {ex.Message}");
#endif
                throw;
            }
        }


        /// <summary>
        /// Gets a service of type T from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the requested service type.</returns>
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>() ??
                throw new InvalidOperationException($"Service {typeof(T)} not found");
        }
    }
}
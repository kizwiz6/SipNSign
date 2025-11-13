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
        private readonly ILoggingService? _logger;

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

                // Get logger service early for crash handling
                _logger = serviceProvider.GetService<ILoggingService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", $"Logger service obtained: {_logger != null}");
#endif

                // Set up global exception handlers BEFORE anything else
                SetupExceptionHandlers();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Exception handlers registered");
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
                Debug.WriteLine($"Critical error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Sets up global exception handlers to catch unhandled exceptions
        /// </summary>
        private void SetupExceptionHandlers()
        {
            // Handle unhandled exceptions from any thread
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Handle unhandled exceptions from tasks
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            _logger?.Debug("Global exception handlers registered");
            Debug.WriteLine("Global exception handlers registered");
        }

        /// <summary>
        /// Handles unhandled exceptions from the main thread
        /// </summary>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                var errorMessage = $"UNHANDLED EXCEPTION: {exception.Message}";
                var stackTrace = $"Stack Trace: {exception.StackTrace}";
                var innerException = exception.InnerException != null ? $"Inner Exception: {exception.InnerException.Message}" : "";

                _logger?.Error(errorMessage);
                _logger?.Error(stackTrace);
                if (!string.IsNullOrEmpty(innerException))
                {
                    _logger?.Error(innerException);
                }

                // Log to debug console
                Debug.WriteLine("==================== UNHANDLED EXCEPTION ====================");
                Debug.WriteLine($"Exception: {exception.Message}");
                Debug.WriteLine($"Type: {exception.GetType().Name}");
                Debug.WriteLine($"Stack Trace: {exception.StackTrace}");
                Debug.WriteLine($"Is Terminating: {e.IsTerminating}");
                if (exception.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {exception.InnerException.Message}");
                    Debug.WriteLine($"Inner Stack Trace: {exception.InnerException.StackTrace}");
                }
                Debug.WriteLine("============================================================");

#if ANDROID
                Android.Util.Log.Error("SipNSignApp", "==================== UNHANDLED EXCEPTION ====================");
                Android.Util.Log.Error("SipNSignApp", $"Exception: {exception.Message}");
                Android.Util.Log.Error("SipNSignApp", $"Type: {exception.GetType().Name}");
                Android.Util.Log.Error("SipNSignApp", $"Stack Trace: {exception.StackTrace}");
                Android.Util.Log.Error("SipNSignApp", $"Is Terminating: {e.IsTerminating}");
                Android.Util.Log.Error("SipNSignApp", "============================================================");
#endif

                // Try to save the error to preferences for later review
                try
                {
                    var crashLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
                    if (exception.InnerException != null)
                    {
                        crashLog += $"\nInner: {exception.InnerException.Message}\n{exception.InnerException.StackTrace}";
                    }

                    var existingLogs = Preferences.Get("crash_logs", "");
                    Preferences.Set("crash_logs", existingLogs + "\n\n" + crashLog);

                    Debug.WriteLine("Crash log saved to preferences");
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine($"Failed to save crash log: {logEx.Message}");
                }

                if (e.IsTerminating)
                {
                    // App is about to crash, try to show error to user
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (Current?.MainPage != null)
                            {
                                await Current.MainPage.DisplayAlert(
                                    "Critical Error",
                                    "The application encountered a critical error and needs to close. The error has been logged.",
                                    "OK");
                            }
                        }
                        catch
                        {
                            // If we can't show dialog, at least the error is logged
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Handles unhandled exceptions from background tasks
        /// </summary>
        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.Error($"UNOBSERVED TASK EXCEPTION: {e.Exception.Message}");
            _logger?.Error($"Stack Trace: {e.Exception.StackTrace}");

            Debug.WriteLine("==================== UNOBSERVED TASK EXCEPTION ====================");
            Debug.WriteLine($"Exception: {e.Exception.Message}");
            Debug.WriteLine($"Stack Trace: {e.Exception.StackTrace}");
            Debug.WriteLine("==================================================================");

#if ANDROID
            Android.Util.Log.Error("SipNSignApp", "==================== UNOBSERVED TASK EXCEPTION ====================");
            Android.Util.Log.Error("SipNSignApp", $"Exception: {e.Exception.Message}");
            Android.Util.Log.Error("SipNSignApp", $"Stack Trace: {e.Exception.StackTrace}");
            Android.Util.Log.Error("SipNSignApp", "==================================================================");
#endif

            // Mark the exception as observed to prevent app crash
            e.SetObserved();

            // Try to save the error
            try
            {
                var crashLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TASK: {e.Exception.Message}\n{e.Exception.StackTrace}";
                var existingLogs = Preferences.Get("crash_logs", "");
                Preferences.Set("crash_logs", existingLogs + "\n\n" + crashLog);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Failed to save task exception: {logEx.Message}");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            _logger?.Debug("App started");
            Debug.WriteLine("App started");

            // Check for previous crash logs
            CheckForPreviousCrashes();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _logger?.Debug("App going to sleep");
            Debug.WriteLine("App going to sleep");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _logger?.Debug("App resumed");
            Debug.WriteLine("App resumed");
        }

        /// <summary>
        /// Checks if there were any crashes in the previous session
        /// </summary>
        private void CheckForPreviousCrashes()
        {
            try
            {
                var crashLogs = Preferences.Get("crash_logs", "");
                if (!string.IsNullOrEmpty(crashLogs))
                {
                    _logger?.Error($"Previous session crash logs found:\n{crashLogs}");
                    Debug.WriteLine($"Previous crash logs found: {crashLogs.Length} characters");

#if ANDROID
                    Android.Util.Log.Warn("SipNSignApp", "Previous crash logs detected");
#endif

                    // Show to user on main thread
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (Current?.MainPage != null)
                            {
                                var result = await Current.MainPage.DisplayAlert(
                                    "Previous Crash Detected",
                                    "The app crashed in the previous session. Would you like to view the crash report?",
                                    "View", "Dismiss");

                                if (result == true)
                                {
                                    var displayLog = crashLogs.Length > 1000
                                        ? crashLogs.Substring(crashLogs.Length - 1000) + "\n\n[Truncated - showing last 1000 chars]"
                                        : crashLogs;

                                    await Current.MainPage.DisplayAlert(
                                        "Crash Report",
                                        displayLog,
                                        "OK");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error showing crash dialog: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking previous crashes: {ex.Message}");
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
                Debug.WriteLine($"Error creating window: {ex.Message}");
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
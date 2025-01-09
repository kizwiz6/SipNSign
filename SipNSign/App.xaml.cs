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
                _serviceProvider = serviceProvider;
                InitializeComponent();

                // Get theme service and apply saved theme
                var themeService = serviceProvider.GetService<IThemeService>();
                if (themeService != null)
                {
                    var savedTheme = themeService.GetCurrentTheme();
                    themeService.SetTheme(savedTheme);
                }

                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = serviceProvider.GetRequiredService<AppShell>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(_serviceProvider.GetRequiredService<AppShell>())
            {
                Title = "SipNSign"
            };
            return window;
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
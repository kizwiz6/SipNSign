using com.kizwiz.sipnsign.Pages;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using com.kizwiz.sipnsign.Services;
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
        private readonly Color _navBarColor = Color.FromArgb("#007BFF");
        private readonly Color _navBarTextColor = Color.FromArgb("#FFFFFF");

        /// <summary>
        /// Initialises a new instance of the <see cref="App"/> class.
        /// This constructor retrieves the user's saved theme preference,
        /// sets the initial application theme, and configures dependency injection.
        /// </summary>
        public App(IServiceProvider serviceProvider)
        {
            try
            {
                _serviceProvider = serviceProvider;
                InitializeComponent();

                var shell = new AppShell(serviceProvider);
                shell.FlyoutBackgroundColor = _navBarColor;
                Shell.SetNavBarHasShadow(shell, false);
                Shell.SetTitleColor(shell, _navBarTextColor);
                MainPage = shell;

                // Background video initialization
                Task.Run(async () =>
                {
                    try
                    {
                        var videoService = _serviceProvider.GetRequiredService<IVideoService>();
                        await videoService.InitializeVideos();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error initialising videos: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex}");
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
            return _serviceProvider.GetService<T>();
        }
    }
}
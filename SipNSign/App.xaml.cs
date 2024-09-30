namespace SipNSign;

/// <summary>
/// Represents the main application for the SipNSign project.
/// Initializes the application and sets the main page.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
	{
        // Set the main page to AppShell
        MainPage = new AppShell();
	}
}

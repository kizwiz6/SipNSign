using com.kizwiz.sipnsign;
using com.kizwiz.sipnsign.Pages;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace com.kizwiz.sipnsign
{
    /// <summary>
    /// Represents the main application for the SipNSign project.
    /// Initializes the application and sets the main page.
    /// </summary>
    public partial class App : Application // Inherit from Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
            MainPage = new GamePage();
        }
    }
}
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace com.kizwiz.signwiz;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

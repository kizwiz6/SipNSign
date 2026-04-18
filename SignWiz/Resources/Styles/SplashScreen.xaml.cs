namespace com.kizwiz.signwiz.Resources.Styles;

public partial class SplashScreen : ContentPage
{
	private readonly IServiceProvider _serviceProvider;

	public SplashScreen(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		_serviceProvider = serviceProvider;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await Task.Delay(3000);

		MainThread.BeginInvokeOnMainThread(() =>
		{
			Application.Current!.Windows[0].Page = _serviceProvider.GetRequiredService<AppShell>();
		});
	}
}
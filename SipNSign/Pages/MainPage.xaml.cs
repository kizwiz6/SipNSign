namespace com.kizwiz.sipnsign;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            // Log the exception (you can use any logging mechanism)
            Console.WriteLine($"Unhandled exception during initialization: {ex.Message}");
            // Optionally, show a user-friendly message
            Content = new Label
            {
                Text = "An error occurred while initializing the page.",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
        }
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
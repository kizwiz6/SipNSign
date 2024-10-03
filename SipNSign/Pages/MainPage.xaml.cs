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
            // Log the exception
            Console.WriteLine($"Unhandled exception during initialization: {ex.Message}");

            // Create a new Grid for layout instead of StackLayout and deprecated options
            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            grid.Children.Add(new Label
            {
                Text = "An error occurred while initializing the page.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            });

            Content = grid;
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

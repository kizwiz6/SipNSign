using com.kizwiz.signwiz.ViewModels;
using com.kizwiz.signwiz.Services;
using System.Diagnostics;

namespace com.kizwiz.signwiz.Pages;

public partial class StorePage : ContentPage
{
    private readonly StoreViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public StorePage(IServiceProvider serviceProvider)
    {
        try
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _viewModel = new StoreViewModel(serviceProvider);
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing StorePage: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Handles when a store purchase button is loaded - sets colors immediately
    /// </summary>
    private void OnStoreButtonLoaded(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Debug.WriteLine($"=== OnStoreButtonLoaded: Text='{button.Text}', IsEnabled={button.IsEnabled}, CommandParameter={button.CommandParameter} ===");

            // Subscribe to property changes
            button.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(Button.IsEnabled))
                {
                    Debug.WriteLine($">>> Button CommandParameter='{button.CommandParameter}' Text='{button.Text}' IsEnabled changed to: {button.IsEnabled}");

                    // WORKAROUND: If text is "OWNED" but IsEnabled is True, force it to False
                    if (button.Text == "OWNED" && button.IsEnabled)
                    {
                        Debug.WriteLine($"!!! FORCING IsEnabled=False for OWNED button (was incorrectly set to True)");
                        button.IsEnabled = false;
                        return; // This will trigger another PropertyChanged event with the correct state
                    }

                    MainThread.BeginInvokeOnMainThread(() => UpdateStoreButtonAppearance(button));
                }
                // CRITICAL: Also react to Text changes (binding applies AFTER Loaded)
                else if (args.PropertyName == nameof(Button.Text))
                {
                    Debug.WriteLine($"=== Button CommandParameter='{button.CommandParameter}' Text changed to '{button.Text}' ===");

                    // WORKAROUND: If text is "OWNED", force the button to disabled state
                    if (button.Text == "OWNED" && button.IsEnabled)
                    {
                        Debug.WriteLine($"!!! FORCING IsEnabled=False for OWNED button (Text changed)");
                        button.IsEnabled = false;
                    }

                    MainThread.BeginInvokeOnMainThread(() => UpdateStoreButtonAppearance(button));
                }
                // Track Background changes to see if something is overriding us
                else if (args.PropertyName == nameof(Button.Background))
                {
                    Debug.WriteLine($"!!! Button '{button.Text}' Background changed externally to: {button.Background}");
                }
                // Track TextColor changes
                else if (args.PropertyName == nameof(Button.TextColor))
                {
                    Debug.WriteLine($"!!! Button '{button.Text}' TextColor changed externally to: {button.TextColor}");
                }
            };

            // Set initial appearance immediately
            // Also check if this is an OWNED button that needs to be disabled
            if (button.Text == "OWNED" && button.IsEnabled)
            {
                Debug.WriteLine($"!!! Initial load: FORCING IsEnabled=False for OWNED button");
                button.IsEnabled = false;
            }

            MainThread.BeginInvokeOnMainThread(() => UpdateStoreButtonAppearance(button));
        }
    }

    /// <summary>
    /// Updates the button appearance based on its enabled state
    /// </summary>
    private void UpdateStoreButtonAppearance(Button button)
    {
        try
        {
            Debug.WriteLine($"=== UpdateStoreButtonAppearance START ===");
            Debug.WriteLine($"  CommandParameter: {button.CommandParameter}");
            Debug.WriteLine($"  Text: '{button.Text}'");
            Debug.WriteLine($"  IsEnabled: {button.IsEnabled}");
            Debug.WriteLine($"  Current Background: {button.Background}");
            Debug.WriteLine($"  Current TextColor: {button.TextColor}");

            // CRITICAL: Check if text is missing
            if (string.IsNullOrEmpty(button.Text))
            {
                Debug.WriteLine($"  !!! WARNING: Button Text is empty!");
            }

            if (button.IsEnabled)
            {
                // Enabled buttons: use Primary color with solid brush
                Color primaryColor = Application.Current?.Resources.TryGetValue("Primary", out var primaryRes) == true && primaryRes is Color pc
                    ? pc
                    : Color.FromArgb("#512BD4"); // Fallback primary color

                // CRITICAL: Clear gradient background using ClearValue to remove style setters
                button.ClearValue(Button.BackgroundProperty);
                button.Background = new SolidColorBrush(primaryColor);

                // Determine text color based on background brightness (for better contrast)
                float r = primaryColor.Red;
                float g = primaryColor.Green;
                float b = primaryColor.Blue;
                float luminance = 0.299f * r + 0.587f * g + 0.114f * b;

                // If background is bright (luminance > 0.5), use dark text; otherwise use white text
                Color textColor = luminance > 0.5f ? Colors.Black : Colors.White;
                button.ClearValue(Button.TextColorProperty);
                button.TextColor = textColor;
                button.Opacity = 1.0;

                Debug.WriteLine($"  ✓ SET TO ENABLED: Background={primaryColor}, TextColor={textColor}, Luminance={luminance:F2}");
            }
            else
            {
                // Disabled buttons (OWNED): use Secondary color or fallback gray
                Color ownedColor = Application.Current?.Resources.TryGetValue("Secondary", out var secRes) == true && secRes is Color sc
                    ? sc
                    : Color.FromArgb("#A0A0A0"); // Fallback gray

                button.ClearValue(Button.BackgroundProperty);
                button.Background = new SolidColorBrush(ownedColor);

                // Determine text color based on background brightness
                float r = ownedColor.Red;
                float g = ownedColor.Green;
                float b = ownedColor.Blue;
                float luminance = 0.299f * r + 0.587f * g + 0.114f * b;

                Color textColor = luminance > 0.5f ? Colors.Black : Colors.White;
                button.ClearValue(Button.TextColorProperty);
                button.TextColor = textColor;
                button.Opacity = 0.7;

                Debug.WriteLine($"  ✓ SET TO DISABLED (OWNED): Background={ownedColor}, TextColor={textColor}, Luminance={luminance:F2}");
            }

            Debug.WriteLine($"  FINAL State: Text='{button.Text}', Background={button.Background}, TextColor={button.TextColor}");
            Debug.WriteLine($"=== UpdateStoreButtonAppearance END ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"!!! ERROR in UpdateStoreButtonAppearance: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Handles when the OWNED button is loaded - sets colors for disabled state
    /// </summary>
    private void OnOwnedButtonLoaded(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Debug.WriteLine($"=== OnOwnedButtonLoaded: OWNED button ===");

            // OWNED button: use Secondary color or gray
            Color secondaryColor = Application.Current?.Resources.TryGetValue("Secondary", out var secRes) == true && secRes is Color sc
                ? sc
                : Color.FromArgb("#A0A0A0"); // Fallback gray

            MainThread.BeginInvokeOnMainThread(() =>
            {
                button.Background = null;
                button.BackgroundColor = secondaryColor;

                // Determine text color based on background brightness
                float r = secondaryColor.Red;
                float g = secondaryColor.Green;
                float b = secondaryColor.Blue;
                float luminance = 0.299f * r + 0.587f * g + 0.114f * b;

                button.TextColor = luminance > 0.5f ? Colors.Black : Colors.White;
                button.Opacity = 0.8;
                Debug.WriteLine($"OWNED button: Secondary={secondaryColor}, Luminance={luminance:F2}, TextColor={button.TextColor}");
            });
        }
    }

    /// <summary>
    /// DEBUG: Clears all purchases and refreshes the store
    /// </summary>
    private async void OnClearPurchasesClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Clear All Purchases?",
            "This will reset all purchased packs to unpurchased state. This is for testing only.",
            "Clear", "Cancel");

        if (confirm)
        {
            try
            {
                // Get the IAP service from the injected service provider
                var iapService = _serviceProvider.GetService(typeof(IIAPService)) as IIAPService;

                if (iapService != null)
                {
                    iapService.ClearAllPurchases();
                    Debug.WriteLine("=== All purchases cleared ===");

                    // Refresh the store
                    await _viewModel.InitializeAsync();

                    await DisplayAlert("Success", "All purchases have been cleared.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing purchases: {ex.Message}");
                await DisplayAlert("Error", "Failed to clear purchases.", "OK");
            }
        }
    }
}
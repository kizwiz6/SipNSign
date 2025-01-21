using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

public partial class StorePage : ContentPage
{
    private readonly StoreViewModel _viewModel;

    public StorePage(IServiceProvider serviceProvider)
    {
        try
        {
            InitializeComponent();
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
}
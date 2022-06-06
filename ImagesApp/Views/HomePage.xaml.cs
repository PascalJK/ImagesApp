using ImagesApp.ViewModels;
using Xamarin.Forms;

namespace ImagesApp.Views;
public partial class HomePage : ContentPage
{

    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }
}

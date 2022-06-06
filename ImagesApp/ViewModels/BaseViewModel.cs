using CommunityToolkit.Mvvm.ComponentModel;

namespace ImagesApp.ViewModels;
public partial class BaseViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    [ObservableProperty] bool isBusy;

    [ObservableProperty] string title = string.Empty;
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;
    }
}
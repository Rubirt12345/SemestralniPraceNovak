using Avalonia.Controls;
using SemestralniPraceNovak.ViewModels;

namespace SemestralniPraceNovak
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext == null)
            {
                DataContext = new MainWindowViewModel();
            }
        }
    }
}
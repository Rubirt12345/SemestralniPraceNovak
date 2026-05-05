using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SemestralniPraceNovak.Controls
{
    public partial class BracketControl : UserControl
    {
        public BracketControl()
        {
            InitializeComponent();
        }

       
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
    }
}
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicUI
{
    
    
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog
            {
                Title = "Hi there",
                Content = "You clicked the button!",
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };
            _ = dlg.ShowAsync();
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

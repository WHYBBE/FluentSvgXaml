using System.Windows;
using System.Windows.Controls;

namespace FluentSvgXaml.Pages;

/// <summary>
/// Interaction logic for StartPage.xaml
/// </summary>
public partial class StartPage : Page
{
    public StartPage()
    {
        InitializeComponent();

        // Reset the dimensions...
        this.Width = Double.NaN;
        this.Height = Double.NaN;

        this.Loaded += OnStartPageLoaded;
    }

    private void OnStartPageLoaded(object sender, RoutedEventArgs e)
    {
    }
}

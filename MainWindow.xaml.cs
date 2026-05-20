using FluentSvgXaml.Core;
using FluentSvgXaml.Pages;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FluentSvgXaml;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IObserver
{
    #region Private Fields

    private int _startTabIndex;

    public int StartTabIndex
    {
        get => _startTabIndex;
        set => _startTabIndex = value;
    }
    private int _operationCount;
    private readonly ConverterOptions _options;

    private readonly OptionsPage _optionsPage;
    private readonly FileConverterPage _filesPage;
    private readonly FileListConverterPage _filesListPage;
    private readonly DirectoryConverterPage _directoriesPage;

    #endregion

    #region Constructors and Destructor

    public MainWindow()
    {
        InitializeComponent();

        this.MinWidth = 640;
        this.MinHeight = 700;

        this.Width = 720;
        this.Height = 700;

        _startTabIndex = 0;

        _options = new ConverterOptions();

        _options.PropertyChanged += OnOptionsPropertyChanged;

        var theApp = Application.Current as App;
        if (theApp?.CommandLines != null)
        {
            var commandLines = theApp.CommandLines;
            if (!commandLines.IsEmpty)
            {
                _options.Update(commandLines);
            }

            if (!string.IsNullOrWhiteSpace(commandLines.SourceFile) && File.Exists(commandLines.SourceFile))
                _startTabIndex = 1;
            else if (!string.IsNullOrWhiteSpace(commandLines.SourceDir) && Directory.Exists(commandLines.SourceDir))
                _startTabIndex = 3;
            else if (commandLines.SourceFiles?.Count > 0)
                _startTabIndex = 2;
        }

        _filesPage = new FileConverterPage
        {
            Options = _options,
            ParentFrame = filesFrame
        };
        _filesPage.Subscribe(this);

        filesFrame.Content = _filesPage;

        _filesListPage = new FileListConverterPage
        {
            Options = _options,
            ParentFrame = filesListFrame
        };
        _filesListPage.Subscribe(this);

        filesListFrame.Content = _filesListPage;

        _directoriesPage = new DirectoryConverterPage
        {
            Options = _options,
            ParentFrame = directoriesFrame
        };
        _directoriesPage.Subscribe(this);

        directoriesFrame.Content = _directoriesPage;

        _optionsPage = new OptionsPage
        {
            Options = _options
        };

        optionsFrame.Content = _optionsPage;

        Loaded += OnWindowLoaded;
        Unloaded += OnWindowUnloaded;

        Closing += OnWindowClosing;
    }

    #endregion

    #region Private Event Handlers

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (_options != null)
        {
            var key = _options.VerticalNav ? "VerticalTabControl" : "HorizontalTabControl";
            tabSteps.Template = (ControlTemplate)FindResource(key);
        }
        var startItem = (TabItem)tabSteps.Items[_startTabIndex];
        startItem.IsSelected = true;
        tabSteps.Focus();
    }

    private void OnWindowUnloaded(object sender, RoutedEventArgs e)
    {
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "VerticalNav" && _options != null)
        {
            var key = _options.VerticalNav ? "VerticalTabControl" : "HorizontalTabControl";
            tabSteps.Template = (ControlTemplate)FindResource(key);
        }
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            if (_operationCount > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Conversion process is running on the background.");
                builder.AppendLine("Do you want to stop the conversion process and close this application?");
                var boxResult = MessageBox.Show(builder.ToString(), this.Title,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (boxResult == MessageBoxResult.No)
                {
                    e.Cancel = false;
                    return;
                }

                _filesPage?.Cancel();
                _filesListPage?.Cancel();
                _directoriesPage?.Cancel();
            }
        }
        catch
        {
        }
    }

    private void OnClickClosed(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region IObserver Members

    public void OnStarted(IObservable sender)
    {
        _operationCount++;

        if (sender == _filesPage)
        {
            filesProgressBar.Visibility = Visibility.Visible;
        }
        else if (sender == _filesListPage)
        {
            filesListProgressBar.Visibility = Visibility.Visible;
        }
        else if (sender == _directoriesPage)
        {
            dirsProgressBar.Visibility = Visibility.Visible;
        }
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        _operationCount--;
        Debug.Assert(_operationCount >= 0);

        if (sender == _filesPage)
        {
            filesProgressBar.Visibility = Visibility.Hidden;
        }
        else if (sender == _filesListPage)
        {
            filesListProgressBar.Visibility = Visibility.Hidden;
        }
        else if (sender == _directoriesPage)
        {
            dirsProgressBar.Visibility = Visibility.Hidden;
        }
    }

    #endregion
}

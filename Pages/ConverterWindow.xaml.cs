using FluentSvgXaml.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace FluentSvgXaml.Pages;

/// <summary>
/// Interaction logic for ConverterWindow.xaml
/// </summary>
public partial class ConverterWindow : Window, IObserver
{
    #region Private Fields

    private delegate void ConvertHandler();

    private bool _isConverting;

    private readonly ConverterOptions _options;

    private FileListConverterOutput? _converterOutput;

    #endregion

    #region Constructors and Destructor

    public ConverterWindow()
    {
        InitializeComponent();

        this.MinWidth = 640;
        this.MinHeight = 340;

        this.Width = 640;
        this.Height = 340;

        _options = new ConverterOptions();

        this.Loaded += new RoutedEventHandler(OnWindowLoaded);
        this.Unloaded += new RoutedEventHandler(OnWindowUnloaded);

        this.Closing += new CancelEventHandler(OnWindowClosing);
        this.ContentRendered += new EventHandler(OnWindowContentRendered);
    }

    #endregion

    #region Private Event Handlers

    private void OnWindowContentRendered(object? sender, EventArgs e)
    {
        if (_options == null || !_options.IsValid)
        {
            return;
        }

        var theApp = (App)Application.Current;
        Debug.Assert(theApp != null);
        if (theApp == null)
        {
            return;
        }
        ConverterCommandLines? commandLines = theApp.CommandLines;
        Debug.Assert(commandLines != null);
        if (commandLines == null || commandLines.IsEmpty)
        {
            return;
        }
        IList<string>? sourceFiles = commandLines.SourceFiles;
        if (sourceFiles == null || sourceFiles.Count == 0)
        {
            string? sourceFile = commandLines.SourceFile;
            if (string.IsNullOrWhiteSpace(sourceFile) ||
                !File.Exists(sourceFile))
            {
                return;
            }
            sourceFiles = [sourceFile];
        }

        _isConverting = true;

        _converterOutput ??= new FileListConverterOutput();

        _options.Update(commandLines);

        _converterOutput.Options = _options;
        _converterOutput.Subscribe(this);

        _converterOutput.ContinueOnError = commandLines.ContinueOnError;
        _converterOutput.SourceFiles = sourceFiles;
        _converterOutput.OutputDir = commandLines.OutputDir;

        frameConverter.Content = _converterOutput;

        //_converterOutput.Convert();
        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new ConvertHandler(_converterOutput.Convert));
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void OnWindowUnloaded(object sender, RoutedEventArgs e)
    {
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            if (_isConverting)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Conversion process is running on the background.");
                builder.AppendLine("Do you want to stop the conversion process and close this application?");
                MessageBoxResult boxResult = MessageBox.Show(builder.ToString(), this.Title,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (boxResult == MessageBoxResult.No)
                {
                    e.Cancel = false;
                    return;
                }

                _converterOutput?.Cancel();
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
        progressBar.Visibility = Visibility.Visible;
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        progressBar.Visibility = Visibility.Hidden;

        _isConverting = false;
    }

    #endregion
}

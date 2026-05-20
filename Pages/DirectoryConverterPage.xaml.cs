using FluentSvgXaml.Core;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace FluentSvgXaml.Pages;

/// <summary>
/// Interaction logic for DirectoryConverterPage.xaml
/// </summary>
public partial class DirectoryConverterPage : Page, IObservable, IObserver
{
    #region Private Fields

    private delegate void ConvertHandler();

    private bool _isConverting;
    private bool _isConversionError;
    /// <summary>
    /// Only one observer is expected!
    /// </summary>
    private Brush _titleBkDefault = Brushes.Transparent;
    private IObserver? _observer;
    private ConverterOptions _options = new();

    private DirectoryConverterOutput? _converterOutput;



    private Frame? _parentFrame;

    #endregion

    #region Constructors and Destructor

    public DirectoryConverterPage()
    {
        InitializeComponent();

        // Reset the dimensions...
        this.Width = Double.NaN;
        this.Height = Double.NaN;

        if (_titleBkDefault == null &&
            (statusTitle != null && statusTitle.IsInitialized))
        {
            _titleBkDefault = statusTitle.Background;
        }

        this.Loaded += new RoutedEventHandler(OnPageLoaded);
    }

    #endregion

    #region Public Properties

    public ConverterOptions Options
    {
        get
        {
            return _options;
        }
        set
        {
            _options = value;

            if (_options != null)
            {
                _options.PropertyChanged += new PropertyChangedEventHandler(OnOptionsPropertyChanged);
            }
        }
    }

    public Frame? ParentFrame
    {
        get
        {
            return _parentFrame;
        }
        set
        {
            _parentFrame = value;
        }
    }

    #endregion

    #region Protected Methods

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        if (_titleBkDefault == null)
        {
            _titleBkDefault = statusTitle.Background;
        }
    }

    #endregion

    #region Private Event Handlers

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Debug.Assert(_options != null);

        if (!_isConversionError)
        {
            this.UpdateStatus();
        }
    }

    private void OnDirTextChanged(object sender, TextChangedEventArgs e)
    {
        this.UpdateStatus();
    }

    private void OnSourceDirClick(object sender, RoutedEventArgs e)
    {
        string sourceDir = txtSourceDir.Text.Trim();
        if (string.IsNullOrWhiteSpace(sourceDir) || Directory.Exists(sourceDir) == false)
        {
            sourceDir = Environment.CurrentDirectory;
        }

        var dlg = new OpenFolderDialog
        {
            Title = "Select the source directory of the SVG files",
            InitialDirectory = sourceDir
        };
        if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FolderName))
        {
            txtSourceDir.Focus();
            txtSourceDir.Text = dlg.FolderName;
        }
    }

    private void OnOutputDirClick(object sender, RoutedEventArgs e)
    {
        string sourceDir = txtSourceDir.Text.Trim();
        if (string.IsNullOrWhiteSpace(sourceDir) || Directory.Exists(sourceDir) == false)
        {
            sourceDir = Environment.CurrentDirectory;
        }

        var dlg = new OpenFolderDialog
        {
            Title = "Select the output directory for the converted file",
            InitialDirectory = sourceDir
        };
        if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FolderName))
        {
            txtOutputDir.Focus();
            txtOutputDir.Text = dlg.FolderName;
        }
    }

    private void OnConvertClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(_parentFrame != null);
        if (_parentFrame == null)
        {
            return;
        }
        _isConverting = true;
        _isConversionError = false;
        btnConvert.IsEnabled = false;

        if (_converterOutput == null)
        {
            _converterOutput = new DirectoryConverterOutput();
        }
        _converterOutput.Options = _options;
        if (chkRecursive.IsChecked != null)
        {
            _converterOutput.Recursive = chkRecursive.IsChecked.Value;
        }
        if (chkContinueOnError.IsChecked != null)
        {
            _converterOutput.ContinueOnError = chkContinueOnError.IsChecked.Value;
        }
        _converterOutput.Subscribe(this);

        _converterOutput.SourceDir = txtSourceDir.Text;
        _converterOutput.OutputDir = txtOutputDir.Text;

        _parentFrame.Content = _converterOutput;

        //_converterOutput.Convert();
        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new ConvertHandler(_converterOutput.Convert));
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _isConversionError = false;
    }

    #endregion

    #region Private Methods

    private void UpdateStatus()
    {
        if (_isConverting)
        {
            this.UpdateStatus("Converting",
                "The conversion process is currently running, please wait...", false);
            return;
        }

        bool isValid = false;

        if (_options.IsValid)
        {
            string sourceDir = txtSourceDir.Text.Trim();
            string outputDir = txtOutputDir.Text.Trim();
            bool isReadOnlyOutputDir = false;
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                try
                {
                    string? rootDir = Path.GetPathRoot(outputDir);
                    if (!string.IsNullOrWhiteSpace(rootDir))
                    {
                        DriveInfo drive = new DriveInfo(rootDir);
                        if (!drive.IsReady || drive.DriveType == DriveType.CDRom
                            || drive.DriveType == DriveType.Unknown)
                        {
                            isReadOnlyOutputDir = true;
                        }
                    }
                }
                catch
                {
                }
            }
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                this.UpdateStatus("Conversion: Not Ready",
                    "Select an input directory of SVG files for conversion.", false);
            }
            else if (Directory.Exists(sourceDir))
            {
                if (isReadOnlyOutputDir)
                {
                    this.UpdateStatus("Error: Output Directory",
                        "The output directory is either invalid or read-only. Please select a different output directory.", true);
                }
                else
                {
                    bool isReadOnlySource = false;
                    try
                    {
                        string? rootDir = Path.GetPathRoot(outputDir);
                        if (!string.IsNullOrWhiteSpace(rootDir))
                        {
                            DriveInfo drive = new DriveInfo(rootDir);
                            if (!drive.IsReady || drive.DriveType == DriveType.CDRom
                                || drive.DriveType == DriveType.Unknown)
                            {
                                isReadOnlySource = true;
                            }
                        }
                    }
                    catch
                    {
                    }
                    if (isReadOnlySource && string.IsNullOrWhiteSpace(outputDir))
                    {
                        this.UpdateStatus("Required: Output Directory",
                            "For the read-only source directory, an output directory is required and must be specified.", true);
                    }
                    else
                    {
                        this.UpdateStatus("Conversion: Ready",
                            "Click the Convert button to convert the SVG files in the source directory.", false);

                        isValid = true;
                    }
                }
            }
            else
            {
                this.UpdateStatus("Error: Source Directory",
                    "The specified source directory is either invalid or does not exists.",
                    true);
            }
        }
        else
        {
            this.UpdateStatus("Error: Options", _options.Message, true);
        }

        btnConvert.IsEnabled = isValid;
    }

    private void UpdateStatus(string title, string text, bool isError)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        statusTitle.Background = isError ? Brushes.Red : _titleBkDefault;
        statusTitle.Foreground = isError ? Brushes.White : Brushes.Black;

        statusTitle.Text = title;
        statusText.Text = text;
    }

    #endregion

    #region IObservable Members

    public void Cancel()
    {
        if (_converterOutput != null)
        {
            _converterOutput.Cancel();
        }
    }

    public void Subscribe(IObserver observer)
    {
        _observer = observer;
    }

    #endregion

    #region IObserver Members

    public void OnStarted(IObservable sender)
    {
        _isConverting = true;

        progressBar.Visibility = Visibility.Visible;

        this.UpdateStatus();

        if (_observer != null)
        {
            _observer.OnStarted(this);
        }
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        _isConverting = false;

        progressBar.Visibility = Visibility.Hidden;

        this.UpdateStatus();

        if (_observer != null)
        {
            _observer.OnCompleted(this, isSuccessful);
        }

        _isConversionError = isSuccessful ? false : true;

        if (isSuccessful)
        {
            this.UpdateStatus("Conversion: Successful",
                "The conversion of the specified directory is completed successfully.", false);
        }
        else
        {
            this.UpdateStatus("Conversion: Failed",
                "The conversion of the specified directory failed, see the output for further information.", true);
        }
    }

    #endregion
}

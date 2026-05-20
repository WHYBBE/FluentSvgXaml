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
/// Interaction logic for FileConverterPage.xaml
/// </summary>
public partial class FileConverterPage : Page, IObservable, IObserver
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
    private FileConverterOutput? _converterOutput;

    #endregion

    #region Constructors and Destructor

    public FileConverterPage()
    {
        InitializeComponent();

        // Reset the dimensions...
        this.Width = Double.NaN;
        this.Height = Double.NaN;

        this.Loaded += OnPageLoaded;

        if (_titleBkDefault == null &&
            (statusTitle != null && statusTitle.IsInitialized))
        {
            _titleBkDefault = statusTitle.Background;
        }
    }

    #endregion

    #region Public Properties

    public ConverterOptions Options
    {
        get;
        set
        {
            field = value;
            field?.PropertyChanged += OnOptionsPropertyChanged;
        }
    } = new();

    public Frame? ParentFrame { get; set; }

    #endregion

    #region Protected Methods

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        _titleBkDefault ??= statusTitle.Background;
    }

    #endregion

    #region Private Event Handlers

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Debug.Assert(Options != null);

        if (!_isConversionError)
        {
            UpdateStatus();
        }
    }

    private void OnSourceOutputTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateStatus();
    }

    private void OnSourceFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data is DataObject item && item.ContainsFileDropList())
        {
            foreach (string? filePath in item.GetFileDropList())
            {
                txtSourceFile.Text = filePath;
                break;  // only a single file conversion is supported...
            }
        }
    }

    private void OnSourceFilePreviewDragEnter(object sender, DragEventArgs e)
    {
        bool dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();
        if (dropPossible)
        {
            e.Effects = DragDropEffects.Copy;
        }
    }

    private void OnSourceFilePreviewDragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
        // this will remove the watermark...
        txtSourceFile.Focus();
    }

    private void OnSourceFileClick(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = false,
            Filter = "SVG Files|*.svg;*.svgz"
        };
        ;
        dlg.FilterIndex = 1;

        bool? isSelected = dlg.ShowDialog();

        if (isSelected != null && isSelected.Value)
        {
            // this will remove the watermark...
            txtSourceFile.Focus();
            txtSourceFile.Text = dlg.FileName;
        }
    }

    private void OnOutputDirClick(object sender, RoutedEventArgs e)
    {
        string sourceFile = txtSourceFile.Text.Trim();
        string sourceDir = Environment.CurrentDirectory;
        if (!string.IsNullOrWhiteSpace(sourceFile) && File.Exists(sourceFile))
        {
            sourceDir = Path.GetDirectoryName(sourceFile) ?? string.Empty;
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
        Debug.Assert(ParentFrame != null);
        if (ParentFrame == null)
        {
            return;
        }
        _isConverting = true;
        _isConversionError = false;
        btnConvert.IsEnabled = false;

        _converterOutput ??= new FileConverterOutput();
        _converterOutput.Options = Options;
        _converterOutput.Subscribe(this);

        _converterOutput.SourceFile = txtSourceFile.Text;
        _converterOutput.OutputDir = txtOutputDir.Text;

        ParentFrame.Content = _converterOutput;

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

        if (Options.IsValid)
        {
            string sourceFile = txtSourceFile.Text.Trim();
            string outputDir = txtOutputDir.Text.Trim();
            bool isReadOnlyOutputDir = false;
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                try
                {
                    string? rootDir = Path.GetPathRoot(outputDir);
                    if (!string.IsNullOrWhiteSpace(rootDir))
                    {
                        var drive = new DriveInfo(rootDir);
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
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                this.UpdateStatus("Conversion: Not Ready",
                    "Select an input SVG file for conversion.", false);
            }
            else if (File.Exists(sourceFile))
            {
                string fileExt = Path.GetExtension(sourceFile);
                if (string.IsNullOrWhiteSpace(fileExt) ||
                    (!string.Equals(fileExt, ".svg", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fileExt, ".svgz", StringComparison.OrdinalIgnoreCase)))
                {
                    this.UpdateStatus("Error: Source File",
                        "The specified file is not a valid SVG file or the file extension is invalid.",
                        true);
                }
                else if (isReadOnlyOutputDir)
                {
                    this.UpdateStatus("Error: Output Directory",
                        "The output directory is either invalid or read-only. Please select a different output directory.", true);
                }
                else
                {
                    bool isReadOnlySource = false;
                    try
                    {
                        var rootDir = Path.GetPathRoot(outputDir);
                        if (!string.IsNullOrWhiteSpace(rootDir))
                        {
                            var drive = new DriveInfo(rootDir);
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
                        this.UpdateStatus("Conversion: Ready (Local File)",
                            "Click the Convert button to convert the input file.", false);

                        isValid = true;
                    }
                }
            }
            else
            {
                // First, we try check for web source file...
                if (Uri.TryCreate(sourceFile, UriKind.Absolute, out Uri? webUri)
                    && (string.Equals(webUri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)
                    || string.Equals(webUri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal)))
                {
                    if (string.IsNullOrWhiteSpace(outputDir))
                    {
                        this.UpdateStatus("Required: Output Directory",
                            "For the web source file, an output directory is required and must be specified.", true);
                    }
                    else if (isReadOnlyOutputDir)
                    {
                        this.UpdateStatus("Error: Output Directory",
                            "The output directory is either invalid or read-only. Please select a different output directory.", true);
                    }
                    else
                    {
                        this.UpdateStatus("Conversion: Ready (Web File)",
                            "Click the Convert button to convert the input file or the Preview button to preview the output.", false);

                        isValid = true;
                    }
                }
                else
                {
                    this.UpdateStatus("Error: Source File",
                        "The specified source file is either invalid or the file does not exists.",
                        true);
                }
            }
        }
        else
        {
            this.UpdateStatus("Error: Options", Options.Message, true);
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
        _converterOutput?.Cancel();
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

        UpdateStatus();

        _observer?.OnStarted(this);
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        _isConverting = false;

        progressBar.Visibility = Visibility.Hidden;

        UpdateStatus();

        _observer?.OnCompleted(this, isSuccessful);

        _isConversionError = !isSuccessful;

        if (isSuccessful)
        {
            UpdateStatus("Conversion: Successful",
                "The conversion of the specified file is completed successfully.", false);
        }
        else
        {
            UpdateStatus("Conversion: Failed",
                "The conversion of the specified file failed, see the output for further information.", true);
        }
    }

    #endregion
}

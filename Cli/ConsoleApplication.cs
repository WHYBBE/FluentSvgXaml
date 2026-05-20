using FluentSvgXaml.Core;
using System.Diagnostics;
using System.IO;

namespace FluentSvgXaml.Cli;

public sealed class ConsoleApplication(Process process) : IObserver, IObservable
{
    #region Private Fields

    private volatile bool _isConverting;
    private bool _isConversionError;

    private bool _consoleSuccess;
    private bool _startedInConsole;

    private IObserver? _observer;

    private readonly Process _process = process;

    private ConsoleWriter? _writer;
    private ConsoleProgress? _progressBar;
    private ConsoleConverter? _converterOutput;
    private readonly ConverterOptions _options = new();
    private ConverterCommandLines? _commandLines;

    #endregion
    #region Constructors and Destructor

    #endregion

    #region Public Properties

    public ConverterCommandLines? CommandLines
    {
        get
        {
            return _commandLines;
        }
        set
        {
            _commandLines = value;
        }
    }

    #endregion

    #region Public Methods

    public void InitializeComponent(bool startedInConsole, bool isQuiet)
    {
        _startedInConsole = startedInConsole;

        _writer = new ConsoleWriter(isQuiet, ConsoleWriterVerbosity.Normal);

        _progressBar = new ConsoleProgress(isQuiet, _writer);
        this.Subscribe(_progressBar);
    }

    public int Run()
    {
        Debug.Assert(_writer != null);
        Debug.Assert(_process != null);
        Debug.Assert(_commandLines != null);

        _isConverting = false;
        _isConversionError = false;

        bool? isControlling = null;
        try
        {
            if (!_writer.IsQuiet)
            {
                _consoleSuccess = CreateConsole();
                if (!_consoleSuccess)
                {
                    return 1;
                }

                isControlling = Console.TreatControlCAsInput;

                Console.TreatControlCAsInput = false;

                Console.CancelKeyPress += new ConsoleCancelEventHandler(OnConsoleCancelKeyPress);
            }

            _options.Update(_commandLines);

            _converterOutput = this.CreateConverter();
            if (_converterOutput == null)
            {
                return 1;
            }

            // The convert method will simply start the background
            // conversion thread and start processing. It will return
            // immediately...
            bool startedConversion = _converterOutput.Convert(_writer);
            if (!startedConversion)
            {
                return 1;
            }

            _writer.WriteLine("Press Control + C keys to cancel the conversion.");
            while (_isConverting)
            {
                Thread.Sleep(50);
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (_writer != null)
            {
                string message = ex.Message;
                if (string.IsNullOrWhiteSpace(message))
                {
                    _writer.WriteErrorLine(ex.ToString());
                }
                else
                {
                    _writer.WriteErrorLine(message);
                }
            }

            return 1;
        }
        finally
        {
            if (isControlling != null && isControlling.HasValue)
            {
                Console.TreatControlCAsInput = isControlling.Value;

                Console.CancelKeyPress -= new ConsoleCancelEventHandler(OnConsoleCancelKeyPress);
            }

            if (_consoleSuccess)
            {
                this.DestroyConsole();

                _consoleSuccess = false;
            }
        }
    }

    public int Help()
    {
        Debug.Assert(_writer != null);
        Debug.Assert(_process != null);
        Debug.Assert(_commandLines != null);

        _isConverting = false;
        _isConversionError = false;

        try
        {
            if (!_writer.IsQuiet)
            {
                // If not quiet, we will display the progress information
                // to the console window, so try creating or attaching to 
                // existing one...
                _consoleSuccess = CreateConsole();
                if (!_consoleSuccess)
                {
                    return 1;
                }
            }

            if (_consoleSuccess)
            {
                string? usageText = _commandLines?.Usage;
                if (!string.IsNullOrWhiteSpace(usageText))
                {
                    _writer?.WriteLine(usageText);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (_writer != null)
            {
                string message = ex.Message;
                if (string.IsNullOrWhiteSpace(message))
                {
                    _writer.WriteErrorLine(ex.ToString());
                }
                else
                {
                    _writer.WriteErrorLine(message);
                }
            }

            return 1;
        }
        finally
        {
            if (_consoleSuccess)
            {
                this.DestroyConsole();

                _consoleSuccess = false;
            }
        }
    }

    #endregion

    #region Private Methods

    private ConsoleConverter? CreateConverter()
    {
        Debug.Assert(_commandLines != null);
        if (_commandLines == null)
        {
            return null;
        }

        string? outputDir = _commandLines.OutputDir;

        string? sourceFile = _commandLines.SourceFile;
        if (!string.IsNullOrWhiteSpace(sourceFile) && File.Exists(sourceFile))
        {
            var fileConverter = new ConsoleFileConverter(sourceFile)
            {
                Options = _options,
                OutputDir = outputDir ?? string.Empty
            };

            fileConverter.Subscribe(this);

            return fileConverter;
        }

        string? sourceDir = _commandLines.SourceDir;
        if (!string.IsNullOrWhiteSpace(sourceDir) && Directory.Exists(sourceDir))
        {
            var dirConverter = new ConsoleDirectoryConverter(sourceDir)
            {
                Options = _options,
                OutputDir = outputDir ?? string.Empty,
                Recursive = _commandLines.Recursive,
                ContinueOnError = _commandLines.ContinueOnError
            };

            dirConverter.Subscribe(this);

            return dirConverter;
        }

        IList<string>? sourceFiles = _commandLines.SourceFiles;
        if (sourceFiles != null && sourceFiles.Count != 0)
        {
            var filesConverter = new ConsoleFilesConverter(sourceFiles)
            {
                Options = _options,
                OutputDir = outputDir ?? string.Empty,
                ContinueOnError = _commandLines.ContinueOnError
            };

            filesConverter.Subscribe(this);

            return filesConverter;
        }

        return null;
    }

    static bool CreateConsole()
    {
        try
        {
            bool consoleSuccess = ConverterWindowsAPI.AttachConsole(-1);
            if (!consoleSuccess)
            {
                consoleSuccess = ConverterWindowsAPI.AllocConsole();
                if (consoleSuccess)
                {
                    Console.Title = "SVG-WPF Converter";
                }
            }

            return consoleSuccess;
        }
        catch
        {
            return false;
        }
    }

    public void DestroyConsole()
    {
        try
        {
            this.AppendLine(string.Empty);
            this.AppendLine("Press the Enter key to continue...");

            if (_commandLines != null && _commandLines.BeepOnEnd)
            {
                if (_isConversionError)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Console.Beep(4000, 50);
                        Console.Beep(1000, 25);
                        Thread.Sleep(30);
                    }
                }
                else
                {
                    Console.Beep(2000, 150);
                    Console.Beep(2000, 150);
                    Console.Beep(2000, 150);
                    Console.Beep(1500, 300);
                }
            }

            ConverterWindowsAPI.FreeConsole();
        }
        catch
        {

        }
    }

    private void OnConsoleCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (_isConverting)
        {
            this.Cancel();
        }

        e.Cancel = !_isConverting;
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

        _observer?.OnStarted(this);
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        if (_observer != null)
        {
            _observer.OnCompleted(this, isSuccessful);

            Thread.Sleep(100); // wait a bit...
        }

        if (isSuccessful)
        {
            this.AppendLines("Conversion: Successful",
                "The conversion is completed successfully.", false);
        }
        else
        {
            this.AppendLines("Conversion: Failed",
                "The conversion failed, see the output for further information.", true);
        }

        _isConversionError = !isSuccessful;

        _isConverting = false;
    }

    private IntPtr GetWindow()
    {
        if (_process != null && _process.MainWindowHandle != IntPtr.Zero)
        {
            return _process.MainWindowHandle;
        }

        return ConverterWindowsAPI.GetConsoleWindow();
    }

    private void AppendText(string text)
    {
        if (text == null)
        {
            return;
        }

        _writer?.WriteLine(text);
    }

    private void AppendLine(string text)
    {
        if (text == null)
        {
            return;
        }

        _writer?.WriteLine(text);
    }

    private void AppendLines(string title, string text, bool isError)
    {
        if (text == null)
        {
            return;
        }

        _writer?.WriteLine(text);
    }

    #endregion
}

using Mono.Options;
using System.IO;

namespace FluentSvgXaml.Core;

public sealed class ConverterCommandLines(string[] args)
{
    #region Private Fields

    private bool _beepEnd = false;
    private bool _showHelp = false;
    private bool _showGuiHelp;

    private bool _isRecursive = false;
    private bool _includeRuntime = false;
    private bool _continueOnError = true;
    private bool _textAsGeometry = true;
    private bool _customXamlWriter = true;

    private bool _saveXaml = true;
    private bool _saveZaml = false;

    private ConverterUIOption _ui = ConverterUIOption.Unknown;
    private string _image = string.Empty;

    private string? _usage;

    private List<string>? _sources;
    private string? _sourceFile;
    private string? _sourceDir;
    private string? _outputDir;

    #endregion
    #region Constructors and Destructor

    #endregion

    #region Public Properties

    public bool IsEmpty
    {
        get
        {
            if (Arguments == null || Arguments.Length == 0)
            {
                return true;
            }

            return (string.IsNullOrWhiteSpace(_sourceFile) &&
                (SourceFiles == null || SourceFiles.Count == 0) &&
                string.IsNullOrWhiteSpace(_sourceDir));
        }
    }

    public string[] Arguments { get; } = args;

    public string? OutputDir
    {
        get
        {
            return _outputDir;
        }
        set
        {
            _outputDir = value;
        }
    }

    public bool Recursive
    {
        get
        {
            return _isRecursive;
        }
        set
        {
            _isRecursive = value;
        }
    }

    public bool IncludeRuntime
    {
        get
        {
            return _includeRuntime;
        }
        set
        {
            _includeRuntime = value;
        }
    }

    public bool ContinueOnError
    {
        get
        {
            return _continueOnError;
        }
        set
        {
            _continueOnError = value;
        }
    }

    public bool TextAsGeometry
    {
        get
        {
            return _textAsGeometry;
        }
        set
        {
            _textAsGeometry = value;
        }
    }

    public bool UseCustomXamlWriter
    {
        get
        {
            return _customXamlWriter;
        }
        set
        {
            _customXamlWriter = value;
        }
    }

    public bool SaveXaml
    {
        get
        {
            return _saveXaml;
        }
        set
        {
            _saveXaml = value;
        }
    }

    public bool SaveZaml
    {
        get
        {
            return _saveZaml;
        }
        set
        {
            _saveZaml = value;
        }
    }

    public bool SaveImage
    {
        get
        {
            return IsValidImage(_image);
        }
    }

    public bool ShowHelp
    {
        get
        {
            return _showHelp;
        }
        set
        {
            _showHelp = value;
        }
    }

    public bool ShowGuiHelp
    {
        get
        {
            return _showGuiHelp;
        }
        set
        {
            _showGuiHelp = value;
        }
    }

    public bool BeepOnEnd
    {
        get
        {
            return _beepEnd;
        }
        set
        {
            _beepEnd = value;
        }
    }

    public string Image
    {
        get
        {
            return _image;
        }
        set
        {
            if (IsValidImage(value))
            {
                _image = value;
            }
            else
            {
                _image = string.Empty;
            }
        }
    }

    public ConverterUIOption Ui
    {
        get
        {
            return _ui;
        }
        set
        {
            _ui = value;
        }
    }

    public IList<string>? Sources
    {
        get
        {
            return _sources;
        }
    }

    public string? SourceFile
    {
        get
        {
            return _sourceFile;
        }
    }

    public string? SourceDir
    {
        get
        {
            return _sourceDir;
        }
    }

    public IList<string>? SourceFiles { get; private set; }

    public string? Usage
    {
        get
        {
            return _usage;
        }
    }

    #endregion

    #region Public Methods

    public bool Parse(bool startedInConsole)
    {
        var parser = new OptionSet();
        try
        {
            var sourceSet = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            this.DefineOptions(parser, sourceSet);

            using (var writer = new StringWriter())
            {
                BeginOptionsUsage(writer);
                parser.WriteOptionDescriptions(writer);
                EndOptionsUsage(writer);

                _usage = writer.ToString();
            }

            var listExtra = parser.Parse(Arguments);
            var sourceArgs = new List<string>(sourceSet.Keys);

            if (listExtra != null && listExtra.Count != 0)
            {
                sourceArgs.AddRange(listExtra);
            }

            if (sourceArgs != null && sourceArgs.Count != 0)
            {
                _sources = new List<string>(sourceArgs.Count);

                var sourceFiles = new List<string>();
                var sourceDirs = new List<string>();

                for (int i = 0; i < sourceArgs.Count; i++)
                {
                    string sourcePath = Path.GetFullPath(
                        Environment.ExpandEnvironmentVariables(sourceArgs[i]));

                    if (File.Exists(sourcePath))
                    {
                        // It is a file, but must be SVG to be useful...
                        string fileExt = Path.GetExtension(sourcePath);
                        if (!string.IsNullOrWhiteSpace(fileExt) &&
                            (fileExt.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                            fileExt.Equals(".svgz", StringComparison.OrdinalIgnoreCase)))
                        {
                            sourceFiles.Add(sourcePath);
                        }
                        _sources.Add(sourcePath);
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        // It is a directory...
                        sourceDirs.Add(sourcePath);
                        _sources.Add(sourcePath);
                    }
                }

                int itemCount = _sources.Count;
                if (itemCount == 1)
                {
                    if (sourceFiles.Count == 1)
                    {
                        _sourceFile = sourceFiles[0];
                    }
                    else if (sourceDirs.Count == 1)
                    {
                        _sourceDir = sourceDirs[0];
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "The input source file is not valid.");
                    }
                }
                else if (itemCount > 1)
                {
                    if (sourceFiles.Count > 1)
                    {
                        SourceFiles = sourceFiles;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "The input source file is not valid.");
                    }
                }
            }

            if (startedInConsole && this.IsEmpty)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Private Methods

    private void DefineOptions(OptionSet options,
        IDictionary<string, bool> sourceSet)
    {
        options.Add("s|source=", "Specifies the input source files or directory.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sourceSet[value] = true;
            }
        });
        options.Add("o|output=", "Specifies the output directory.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.OutputDir = value;
            }
        });
        options.Add("r|recursive", "Specifies whether a directory conversion is recursive.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _isRecursive = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("t|runtime", "Specifies whether to include runtime library support.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _includeRuntime = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("e|onError", "Specifies whether to continue conversion when an error occurs in a file.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _continueOnError = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("g|textGeometry", "Specifies whether to render texts as path geometry.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _textAsGeometry = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("c|customXamlWriter", "Specifies whether to use the customized XAML Writer.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _customXamlWriter = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("x|xaml", "Specifies whether to save in uncompressed XAML format.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _saveXaml = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("z|zaml", "Specifies whether to save in compressed XAML format.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _saveZaml = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
        options.Add("i|image=", "Specifies whether to save image and image formats: png, jpeg, tiff, gif, bmp, wdp", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.Image = value!;
            }
        });
        options.Add("u|ui=", "Specifies the user-interface option: none, console or window.", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value.ToLower())
                {
                    case "unknown":
                        this.Ui = ConverterUIOption.Unknown;
                        break;
                    case "null":
                    case "none":
                        this.Ui = ConverterUIOption.None;
                        break;
                    case "console":
                        this.Ui = ConverterUIOption.Console;
                        break;
                    case "window":
                    case "windows":
                        this.Ui = ConverterUIOption.Windows;
                        break;
                }
            }
        });
        options.Add("h|?|help", "Specifies whether to display usage and command-line help.", delegate (string value)
        {
            _showHelp = !string.IsNullOrWhiteSpace(value);
        });
        options.Add("gui-help", "Specifies whether to display help in GUI window.", delegate (string value)
        {
            _showGuiHelp = !string.IsNullOrWhiteSpace(value);
        });
        options.Add("b|beep", "Specifies whether to beep on completion (console only).", delegate (string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                (string.Equals(value, "+", StringComparison.Ordinal)
                    || string.Equals(value, "-", StringComparison.Ordinal)))
            {
                _beepEnd = string.Equals(value, "+", StringComparison.Ordinal);
            }
        });
    }

    private void BeginOptionsUsage(TextWriter writer)
    {
        writer.WriteLine();
        if (Arguments != null && Arguments.Length != 0)
        {
            writer.WriteLine("Argument Count=" + Arguments.Length);
            foreach (string arg in Arguments)
            {
                writer.WriteLine(arg);
            }
        }
        writer.WriteLine("Usage: FluentSvgXaml.exe [options]+");
        writer.WriteLine("Options:");
    }

    static void EndOptionsUsage(TextWriter writer)
    {
        writer.WriteLine();
    }

    private static bool IsValidImage(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return false;
        }
        if (image.Equals("png", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (image.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
            image.Equals("jpg", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (image.Equals("tiff", StringComparison.OrdinalIgnoreCase) ||
            image.Equals("tif", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (image.Equals("bmp", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (image.Equals("wdp", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    #endregion
}

namespace FluentSvgXaml.Cli;

public sealed class ConsoleWriter(bool isQuiet, ConsoleWriterVerbosity verbosity)
{
    private readonly ConsoleWriterVerbosity _verbosity = verbosity;

    public ConsoleWriter()
        : this(false, ConsoleWriterVerbosity.Normal)
    {
    }

    public bool IsQuiet { get; } = isQuiet;

    public object SynchObject { get; } = new();

    public ConsoleWriterVerbosity Verbosity
    {
        get
        {
            return _verbosity;
        }
    }

    public void WriteLine()
    {
        if (IsQuiet)
        {
            return;
        }

        lock (SynchObject)
        {
            Console.WriteLine();
        }
    }

    public void Write(string text)
    {
        if (IsQuiet || text == null)
        {
            return;
        }

        lock (SynchObject)
        {
            Console.Write(text);
        }
    }

    public void WriteProgress(string text)
    {
        if (IsQuiet || text == null)
        {
            return;
        }

        lock (SynchObject)
        {
            //Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(text);
            Console.Write("\b");
            //Console.ResetColor();
        }
    }

    public void WriteLine(string text)
    {
        if (IsQuiet || text == null)
        {
            return;
        }

        lock (SynchObject)
        {
            Console.WriteLine(text);
        }
    }

    public void WriteInfoLine(string text)
    {
        if (IsQuiet || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        lock (SynchObject)
        {
            Console.WriteLine("Info: " + text);
        }
    }

    public void WriteWarnLine(string text)
    {
        if (IsQuiet || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        lock (SynchObject)
        {
            Console.WriteLine("Warn: " + text);
        }
    }

    public void WriteErrorLine(string text)
    {
        if (IsQuiet || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        lock (SynchObject)
        {
            Console.WriteLine("Error: " + text);
        }
    }
}

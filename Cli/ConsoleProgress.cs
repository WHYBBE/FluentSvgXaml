using FluentSvgXaml.Core;

namespace FluentSvgXaml.Cli;

public sealed class ConsoleProgress(bool isQuiet, ConsoleWriter writer) : IObserver
{
    #region Private Fields

    private readonly bool _isQuiet = isQuiet;
    private volatile bool _isStarted;
    private Thread? _thread;
    private readonly ConsoleWriter _writer = writer;

    #endregion
    #region Constructors and Destructor

    #endregion

    #region Public Properties

    #endregion

    #region Private Methods

    private void ThreadProc()
    {
        int counter = 0;
        var frame = new string[]
        {
            "|", "/", "-", "\\", "|", "/", "-", "\\"
        };

        while (true)
        {
            counter++;

            int index = counter % frame.Length;
            _writer.WriteProgress(frame[index]);

            if (counter >= frame.Length)
            {
                counter = 0;
            }

            if (!_isStarted ||
                (_thread != null && _thread.ThreadState == ThreadState.AbortRequested))
            {
                _writer.Write(" ");
                break;
            }
        }
    }

    #endregion

    #region IObserver Members

    public void OnStarted(IObservable sender)
    {
        _isStarted = true;
        if (_isQuiet)
        {
            return;
        }

        _thread = new Thread(ThreadProc)
        {
            IsBackground = true
        };
        _thread.Start();
    }

    public void OnCompleted(IObservable sender, bool isSuccessful)
    {
        _isStarted = false;

        if (_isQuiet || _thread == null)
        {
            return;
        }
        if (_thread != null && _thread.IsAlive)
        {
            try
            {
                _thread.Join(TimeSpan.FromSeconds(5));
            }
            catch
            {
            }
            _thread = null;
        }
    }

    #endregion
}

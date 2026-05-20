using System.ComponentModel;

namespace FluentSvgXaml.Cli;

public sealed class ConsoleWorker
{
    #region Private Fields

    private int _count;
    private readonly int _maxCount;
    private readonly Lock _countProtector;

    private readonly DoWorkEventHandler _eventHandler;

    #endregion

    #region Constructors and Destructor

    public ConsoleWorker()
        : this(1)
    {
    }

    public ConsoleWorker(int maximumCount)
    {
        _countProtector = new Lock();

        _maxCount = maximumCount;
        _eventHandler = new DoWorkEventHandler(this.OnDoWork);
    }

    #endregion

    #region Public Events

    public event DoWorkEventHandler? DoWork;
    public event RunWorkerCompletedEventHandler? RunWorkerCompleted;
    public event ProgressChangedEventHandler? ProgressChanged;

    #endregion

    #region Public Properties

    public bool IsBusy
    {
        get
        {
            lock (_countProtector)
            {
                if (_count >= _maxCount)
                {
                    return true;
                }

                return false;
            }
        }
    }

    public bool CancellationPending { get; private set; }

    #endregion

    #region Public Methods

    public bool RunWorkerAsync(bool abortIfBusy)
    {
        return RunWorkerAsync(abortIfBusy, null);
    }

    public bool RunWorkerAsync(object? argument)
    {
        if (IsBusy)
        {
            return false;
        }
        _count++;

        var args = new DoWorkEventArgs(argument);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public bool RunWorkerAsync()
    {
        if (IsBusy)
        {
            return false;
        }
        _count++;

        var args = new DoWorkEventArgs(null);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public bool RunWorkerAsync(bool abortIfBusy, object? argument)
    {
        if (abortIfBusy && IsBusy)
        {
            return false;
        }
        _count++;

        var args = new DoWorkEventArgs(argument);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public void CancelAsync()
    {
        CancellationPending = true;
    }

    public void ReportProgress(int percentProgress)
    {
        this.OnProgressChanged(new ProgressChangedEventArgs(percentProgress, null));
    }

    public void ReportProgress(int percentProgress, object? userState)
    {
        this.OnProgressChanged(new ProgressChangedEventArgs(percentProgress, userState));
    }

    #endregion

    #region Private Methods

    private void OnDoWork(object? sender, DoWorkEventArgs e)
    {
        if (e.Cancel)
        {
            return;
        }
        DoWork?.Invoke(this, e);
    }

    private void OnProgressChanged(ProgressChangedEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }

    private void RunWorker(DoWorkEventArgs args)
    {
        Exception? error = null;
        try
        {
            _eventHandler(this, args);
        }
        catch (Exception ex)
        {
            error = ex;
        }

        RunWorkerCompleted?.Invoke(this,
            new RunWorkerCompletedEventArgs(args.Result, error, args.Cancel));

        lock (_countProtector)
        {
            _count--;
        }
    }

    #endregion
}

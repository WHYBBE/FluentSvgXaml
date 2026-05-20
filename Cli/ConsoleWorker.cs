using System.ComponentModel;

namespace FluentSvgXaml.Cli;

public sealed class ConsoleWorker
{
    #region Private Fields

    private int _count;
    private int _maxCount;
    private bool _cancelationPending;
    private object _countProtector;

    private DoWorkEventHandler _eventHandler;

    #endregion

    #region Constructors and Destructor

    public ConsoleWorker()
        : this(1)
    {
    }

    public ConsoleWorker(int maximumCount)
    {
        _countProtector = new Object();

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

    public bool CancellationPending
    {
        get
        {
            return _cancelationPending;
        }
    }

    #endregion

    #region Public Methods

    public bool RunWorkerAsync(bool abortIfBusy)
    {
        return this.RunWorkerAsync(abortIfBusy, null);
    }

    public bool RunWorkerAsync(object? argument)
    {
        if (this.IsBusy)
        {
            return false;
        }
        _count++;

        DoWorkEventArgs args = new DoWorkEventArgs(argument);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public bool RunWorkerAsync()
    {
        if (this.IsBusy)
        {
            return false;
        }
        _count++;

        DoWorkEventArgs args = new DoWorkEventArgs(null);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public bool RunWorkerAsync(bool abortIfBusy, object? argument)
    {
        if (abortIfBusy && this.IsBusy)
        {
            return false;
        }
        _count++;

        DoWorkEventArgs args = new DoWorkEventArgs(argument);
        Task.Run(() => RunWorker(args));

        return true;
    }

    public void CancelAsync()
    {
        _cancelationPending = true;
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
        if (this.DoWork != null)
        {
            this.DoWork(this, e);
        }
    }

    private void OnProgressChanged(ProgressChangedEventArgs e)
    {
        if (this.ProgressChanged != null)
        {
            this.ProgressChanged(this, e);
        }
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

        this.RunWorkerCompleted?.Invoke(this,
            new RunWorkerCompletedEventArgs(args.Result, error, args.Cancel));

        lock (_countProtector)
        {
            _count--;
        }
    }

    #endregion
}

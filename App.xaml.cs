using System;
using System.Windows;
using System.Windows.Threading;

using SharpVectors.Converters;

namespace SvgXaml;

public partial class App : Application
{
    public ConverterCommandLines? CommandLines { get; set; }

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        this.DispatcherUnhandledException += OnApplicationUnhandledException;
    }

    private void OnApplicationUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
    }

    public static void DoEvents()
    {
        var frame = new DispatcherFrame(true);
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)(arg =>
            {
                ((DispatcherFrame)arg!).Continue = false;
            }), frame);
        Dispatcher.PushFrame(frame);
    }
}

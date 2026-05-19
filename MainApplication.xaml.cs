using System;
using System.IO;
using System.Threading;

using System.Windows;
using System.Windows.Threading;

namespace SharpVectors.Converters
{
    public partial class MainApplication : Application
    {
        private ConverterCommandLines? _commandLines;

        public MainApplication()
        {
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            this.DispatcherUnhandledException += OnApplicationUnhandledException;
        }

        public ConverterCommandLines? CommandLines
        {
            get => _commandLines;
            set => _commandLines = value;
        }

        public void InitializeComponent(bool mainWindow)
        {
            var fluentDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml")
            };
            this.Resources.MergedDictionaries.Add(fluentDict);
            this.ThemeMode = ThemeMode.System;

            if (mainWindow)
            {
                this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
            else
            {
                this.StartupUri = new Uri("ConverterWindow.xaml", UriKind.Relative);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
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
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object? arg)
            {
                var f = arg as DispatcherFrame;
                f!.Continue = false;
            }, frame);
            Dispatcher.PushFrame(frame);
        }
    }
}

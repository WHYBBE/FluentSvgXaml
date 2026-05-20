using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using FluentSvgXaml.Core;
using FluentSvgXaml.Pages;

namespace FluentSvgXaml
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObserver
    {
        #region Private Fields

        private int _startTabIndex;

        public int StartTabIndex
        {
            get => _startTabIndex;
            set => _startTabIndex = value;
        }
        private int _operationCount;
        private ConverterOptions _options;

        private OptionsPage _optionsPage;
        private FileConverterPage _filesPage;
        private FileListConverterPage _filesListPage;
        private DirectoryConverterPage _directoriesPage;

        #endregion

        #region Constructors and Destructor

        public MainWindow()
        {
            InitializeComponent();

            this.MinWidth  = 640;
            this.MinHeight = 700;

            this.Width     = 720;
            this.Height    = 700;

            _startTabIndex = 0;

            _options = new ConverterOptions();

            _options.PropertyChanged += OnOptionsPropertyChanged;

            var theApp = Application.Current as App;
            if (theApp?.CommandLines != null)
            {
                var commandLines = theApp.CommandLines;
                if (!commandLines.IsEmpty)
                {
                    _options.Update(commandLines);
                }

                if (!string.IsNullOrWhiteSpace(commandLines.SourceFile) && File.Exists(commandLines.SourceFile))
                    _startTabIndex = 1;
                else if (!string.IsNullOrWhiteSpace(commandLines.SourceDir) && Directory.Exists(commandLines.SourceDir))
                    _startTabIndex = 3;
                else if (commandLines.SourceFiles?.Count > 0)
                    _startTabIndex = 2;
            }

            _filesPage = new FileConverterPage();
            _filesPage.Options     = _options;
            _filesPage.ParentFrame = filesFrame;
            _filesPage.Subscribe(this);

            filesFrame.Content = _filesPage;

            _filesListPage = new FileListConverterPage();
            _filesListPage.Options     = _options;
            _filesListPage.ParentFrame = filesListFrame;
            _filesListPage.Subscribe(this);

            filesListFrame.Content = _filesListPage;

            _directoriesPage = new DirectoryConverterPage();
            _directoriesPage.Options     = _options;
            _directoriesPage.ParentFrame = directoriesFrame;
            _directoriesPage.Subscribe(this);

            directoriesFrame.Content = _directoriesPage;

            _optionsPage = new OptionsPage();
            _optionsPage.Options = _options;

            optionsFrame.Content = _optionsPage;

            this.Loaded   += OnWindowLoaded;
            this.Unloaded += OnWindowUnloaded;

            this.Closing  += OnWindowClosing;
        }

        #endregion

        #region Private Event Handlers

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_options != null)
            {
                var key = _options.VerticalNav ? "VerticalTabControl" : "HorizontalTabControl";
                tabSteps.Template = (ControlTemplate)FindResource(key);
            }
            TabItem startItem = (TabItem)tabSteps.Items[_startTabIndex];
            startItem.IsSelected = true;
            tabSteps.Focus();
        }

        private void OnWindowUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "VerticalNav" && _options != null)
            {
                var key = _options.VerticalNav ? "VerticalTabControl" : "HorizontalTabControl";
                tabSteps.Template = (ControlTemplate)FindResource(key);
            }
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            try
            {
                if (_operationCount > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Conversion process is running on the background.");
                    builder.AppendLine("Do you want to stop the conversion process and close this application?");
                    MessageBoxResult boxResult = MessageBox.Show(builder.ToString(), this.Title, 
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, 
                        MessageBoxResult.No);

                    if (boxResult == MessageBoxResult.No)
                    {
                        e.Cancel = false;
                        return;
                    }

                    if (_filesPage != null)
                    {
                        _filesPage.Cancel();
                    }
                    if (_filesListPage != null)
                    {
                        _filesListPage.Cancel();
                    }
                    if (_directoriesPage != null)
                    {
                        _directoriesPage.Cancel();
                    }
                }
            }
            catch
            {                	
            }
        }

        private void OnClickClosed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region IObserver Members

        public void OnStarted(IObservable sender)
        {
            _operationCount++;

            if (sender == _filesPage)
            {
                filesProgressBar.Visibility = Visibility.Visible;
            }
            else if (sender == _filesListPage)
            {
                filesListProgressBar.Visibility = Visibility.Visible;
            }
            else if (sender == _directoriesPage)
            {
                dirsProgressBar.Visibility = Visibility.Visible;
            }
        }

        public void OnCompleted(IObservable sender, bool isSuccessful)
        {
            _operationCount--;
            Debug.Assert(_operationCount >= 0);

            if (sender == _filesPage)
            {
                filesProgressBar.Visibility = Visibility.Hidden;
            }
            else if (sender == _filesListPage)
            {
                filesListProgressBar.Visibility = Visibility.Hidden;
            }
            else if (sender == _directoriesPage)
            {
                dirsProgressBar.Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}

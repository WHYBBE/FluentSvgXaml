using System;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FluentSvgXaml.Pages
{
    public partial class HelpPage : Page
    {
        public HelpPage()
        {
            InitializeComponent();

            this.Width  = Double.NaN;
            this.Height = Double.NaN;

            this.Loaded      += OnHelpPageLoaded;
            this.Unloaded    += OnHelpPageUnloaded;
            this.SizeChanged += OnHelpPageSizeChanged;

            DependencyPropertyDescriptor zoomProperty = 
                DependencyPropertyDescriptor.FromProperty(FlowDocumentReader.ZoomProperty, typeof(FlowDocumentReader));

            zoomProperty.AddValueChanged(this.helpViewer, new EventHandler(OnZoomChanged));
        }

        private void OnHelpPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FlowDocument flowDocument = (FlowDocument)Application.LoadComponent(
                    new Uri("/HelpDocument/ConverterHelp.xaml", UriKind.Relative));
                helpViewer.Document = flowDocument;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "SVG-WPF Converter", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnHelpPageUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnHelpPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void OnZoomChanged(object? sender, EventArgs e)
        {
        }
    }
}

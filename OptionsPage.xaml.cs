using SharpVectors.Converters;
using System;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FluentSvgXaml
{
    public partial class OptionsPage : Page
    {
        private ConverterOptions _options;

        public OptionsPage()
        {
            InitializeComponent();

            this.Width  = Double.NaN;
            this.Height = Double.NaN;

            this.Loaded += OnOptionsPageLoaded;
        }

        public ConverterOptions Options
        {
            get => _options;
            set => _options = value;
        }

        private void OnOptionsPageLoaded(object sender, RoutedEventArgs e)
        {
            if (_options == null)
            {
                _options = new ConverterOptions();
            }

            chkTextAsGeometry.IsChecked = _options.TextAsGeometry;
            chkIncludeRuntime.IsChecked = _options.IncludeRuntime;
            chkVerticalNav.IsChecked   = _options.VerticalNav;

            chkXaml.IsChecked       = _options.GeneralWpf;
            panelXaml.IsEnabled     = _options.GeneralWpf;
            chkSameXaml.IsChecked   = _options.SaveXaml;
            chkSameZaml.IsChecked   = _options.SaveZaml;
            chkXamlWriter.IsChecked = _options.UseCustomXamlWriter;

            chkImage.IsChecked      = _options.GenerateImage;
            panelImage.IsEnabled    = _options.GenerateImage;
            cboImages.SelectedIndex = (int)_options.EncoderType;
        }

        private void OnOptionChanged(object sender, RoutedEventArgs e)
        {
            if (_options == null) return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                if (sender == chkTextAsGeometry)
                    _options.TextAsGeometry = chkTextAsGeometry.IsChecked == true;
                else if (sender == chkIncludeRuntime)
                    _options.IncludeRuntime = chkIncludeRuntime.IsChecked == true;
                else if (sender == chkVerticalNav)
                    _options.VerticalNav = chkVerticalNav.IsChecked == true;
                else if (sender == chkXaml)
                {
                    _options.GeneralWpf = chkXaml.IsChecked == true;
                    panelXaml.IsEnabled = chkXaml.IsChecked == true;
                }
                else if (sender == chkSameXaml)
                    _options.SaveXaml = chkSameXaml.IsChecked == true;
                else if (sender == chkSameZaml)
                    _options.SaveZaml = chkSameZaml.IsChecked == true;
                else if (sender == chkXamlWriter)
                    _options.UseCustomXamlWriter = chkXamlWriter.IsChecked == true;
                else if (sender == chkImage)
                {
                    _options.GenerateImage = chkImage.IsChecked == true;
                    panelImage.IsEnabled = chkImage.IsChecked == true;
                }
                else if (sender == cboImages && cboImages.SelectedIndex >= 0)
                    _options.EncoderType = (ImageEncoderType)cboImages.SelectedIndex;
            });
        }
    }
}

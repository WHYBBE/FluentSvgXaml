using SharpVectors.Converters;
using System;
using System.ComponentModel;

namespace FluentSvgXaml.Core
{
    [Serializable]
    public sealed class ConverterOptions : ICloneable, INotifyPropertyChanged
    {
        #region Private Fields

        private bool _textAsGeometry;
        private bool _includeRuntime;
        private bool _generateImage;
        private bool _generalWpf;
        private bool _saveXaml;
        private bool _saveZaml;
        private bool _customXamlWriter;
        private bool _verticalNav;

        private string _errorMessage;

        private ImageEncoderType _encoderType;

        #endregion

        #region Constructors and Destructor

        public ConverterOptions()
        {
            _textAsGeometry   = true;
            _includeRuntime   = false;
            _generateImage    = false;
            _generalWpf       = true;
            _saveXaml         = true;
            _saveZaml         = false;
            _customXamlWriter = true;
            _verticalNav      = true;
            _errorMessage     = string.Empty;
            _encoderType      = ImageEncoderType.PngBitmap;
        }

        public ConverterOptions(ConverterOptions source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            _textAsGeometry   = source._textAsGeometry;
            _includeRuntime   = source._includeRuntime;
            _generateImage    = source._generateImage;
            _generalWpf       = source._generalWpf;
            _saveXaml         = source._saveXaml;
            _saveZaml         = source._saveZaml;
            _customXamlWriter = source._customXamlWriter;
            _verticalNav      = source._verticalNav;
            _encoderType      = source._encoderType;
            _errorMessage     = source._errorMessage;
        }

        #endregion

        #region Public Properties

        public bool IsValid
        {
            get
            {
                _errorMessage = string.Empty;

                if (!_generateImage && !_generalWpf)
                {
                    _errorMessage = "No output target (XAML or Image) is selected.";

                    return false;
                }
                else if (_generalWpf)
                {   
                    if (!_saveXaml && !_saveZaml)
                    {
                        _errorMessage = "The XAML output target is selected but no file format is specified.";

                        return false;
                    }
                }

                return true;
            }
        }

        public string Message
        {
            get
            {
                return _errorMessage;
            }
        }

        public bool TextAsGeometry
        {
            get => _textAsGeometry;
            set
            {
                if (_textAsGeometry != value)
                {
                    _textAsGeometry = value;
                    this.Notify("TextAsGeometry");
                }
            }
        }

        public bool IncludeRuntime
        {
            get => _includeRuntime;
            set
            {
                if (_includeRuntime != value)
                {
                    _includeRuntime = value;
                    this.Notify("IncludeRuntime");
                }
            }
        }

        public bool GenerateImage
        {
            get => _generateImage;
            set
            {
                if (_generateImage != value)
                {
                    _generateImage = value;
                    this.Notify("GenerateImage");
                }
            }
        }

        public bool GeneralWpf
        {
            get => _generalWpf;
            set
            {
                if (_generalWpf != value)
                {
                    _generalWpf = value;
                    this.Notify("GeneralWpf");
                }
            }
        }

        public bool SaveXaml
        {
            get => _saveXaml;
            set
            {
                if (_saveXaml != value)
                {
                    _saveXaml = value;
                    this.Notify("SaveXaml");
                }
            }
        }

        public bool SaveZaml
        {
            get => _saveZaml;
            set
            {
                if (_saveZaml != value)
                {
                    _saveZaml = value;
                    this.Notify("SaveZaml");
                }
            }
        }

        public bool UseCustomXamlWriter
        {
            get => _customXamlWriter;
            set
            {
                if (_customXamlWriter != value)
                {
                    _customXamlWriter = value;
                    this.Notify("UseCustomXamlWriter");
                }
            }
        }

        public ImageEncoderType EncoderType
        {
            get => _encoderType;
            set
            {
                if (_encoderType != value)
                {
                    _encoderType = value;
                    this.Notify("EncoderType");
                }
            }
        }

        public bool VerticalNav
        {
            get => _verticalNav;
            set
            {
                if (_verticalNav != value)
                {
                    _verticalNav = value;
                    this.Notify("VerticalNav");
                }
            }
        }

        #endregion

        #region Private Methods

        private void Notify(string propertyName)
        {
            PropertyChangedEventHandler? handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region ICloneable Members

        public ConverterOptions Clone()
        {
            ConverterOptions options = new ConverterOptions(this);
            if (_errorMessage != null)
            {
                options._errorMessage = new string(_errorMessage.ToCharArray());
            }

            return options;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Public Methods

        public void Update(ConverterCommandLines commands)
        {
            if (commands == null) return;

            _textAsGeometry    = commands.TextAsGeometry;
            _includeRuntime    = commands.IncludeRuntime;
            _saveXaml          = commands.SaveXaml;
            _saveZaml          = commands.SaveZaml;
            _generateImage     = commands.SaveImage;
            _generalWpf        = _saveXaml || _saveZaml;
            _customXamlWriter  = commands.UseCustomXamlWriter;
            if (_generateImage)
            {
                switch (commands.Image.ToLower())
                {
                    case "bmp":  _encoderType = ImageEncoderType.BmpBitmap; break;
                    case "png":  _encoderType = ImageEncoderType.PngBitmap; break;
                    case "jpeg":
                    case "jpg":  _encoderType = ImageEncoderType.JpegBitmap; break;
                    case "tif":
                    case "tiff": _encoderType = ImageEncoderType.TiffBitmap; break;
                    case "gif":  _encoderType = ImageEncoderType.GifBitmap; break;
                    case "wdp":  _encoderType = ImageEncoderType.WmpBitmap; break;
                }
            }
        }

        #endregion
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GmodAddonCompressor.DataContexts
{
    internal class MainWindowContext : INotifyPropertyChanged
    {
        private string _addonDirectoryPath = string.Empty;
        private string _progressBarText = string.Empty;
        private int _progressBarMinValue = 0;
        private int _progressBarMaxValue = 100;
        private int _progressBarValue = 0;
        private bool _unlockedUI = true;
        private bool _compressVTF = true;
        private bool _compressWAV = true;
        private bool _compressMP3 = true;
        private bool _compressOGG = true;
        private bool _compressJPG = true;
        private bool _compressPNG = true;
        private bool _compressLUA = true;
        private bool _useFFMpegForCompress = true;
        private bool _changeOriginalCodeToMinimalistic = false;
        private bool _reduceExactlyToLimits = false;
        private bool _reduceExactlyToResolution = true;
        private bool _keepImageAspectRatio = true;
        private bool _imageMagickVTFCompress = false;
        private uint _imageSkipWidth = 0;
        private uint _imageSkipHeight = 0;
        private int _wavRate = 22050;
        private int _wavRateListIndex = 0;
        private int _imageReducingResolutionListIndex = 0;
        private int _imageWidthLimitIndex = 10;
        private int _imageHeightLimitIndex = 10;
        private int[] _imageReducingResolutionList = new int[]
        {
            2,
            4,
            6,
            8,
            10,
            12,
        };
        private uint[] _imageSizeLimitList = new uint[]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
            4096,
        };
        private int[] _wavRateList = new int[]
        {
            //44100,
            32000,
            22050,
            16000
        };

        public uint ImageSkipHeight
        {
            get { return _imageSkipHeight; }
            set
            {
                _imageSkipHeight = value;
                OnPropertyChanged();
            }
        }

        public uint ImageSkipWidth
        {
            get { return _imageSkipWidth; }
            set
            {
                _imageSkipWidth = value;
                OnPropertyChanged();
            }
        }

        public uint[] ImageSizeLimitList
        {
            get { return _imageSizeLimitList; }
            set
            {
                _imageSizeLimitList = value;
                OnPropertyChanged();
            }
        }

        public int ImageWidthLimitIndex
        {
            get { return _imageWidthLimitIndex; }
            set
            {
                _imageWidthLimitIndex = value;
                OnPropertyChanged();
            }
        }

        public int ImageHeightLimitIndex
        {
            get { return _imageHeightLimitIndex; }
            set
            {
                _imageHeightLimitIndex = value;
                OnPropertyChanged();
            }
        }

        public int ImageReducingResolutionListIndex
        {
            get { return _imageReducingResolutionListIndex; }
            set
            {
                _imageReducingResolutionListIndex = value;
                OnPropertyChanged();
            }
        }

        public int[] ImageReducingResolutionList
        {
            get { return _imageReducingResolutionList; }
            set
            {
                _imageReducingResolutionList = value;
                OnPropertyChanged();
            }
        }

        public int WavRateListIndex
        {
            get { return _wavRateListIndex; }
            set
            {
                _wavRateListIndex = value;
                OnPropertyChanged();
            }
        }

        public int[] WavRateList
        {
            get { return _wavRateList; }
            set
            {
                _wavRateList = value;
                OnPropertyChanged();
            }
        }

        public int WavRate
        {
            get { return _wavRate; }
            set
            {
                _wavRate = value;
                OnPropertyChanged();
            }
        }

        public bool ImageMagickVTFCompress
        {
            get { return _imageMagickVTFCompress; }
            set
            {
                _imageMagickVTFCompress = value;
                OnPropertyChanged();
            }
        }

        public bool KeepImageAspectRatio
        {
            get { return _keepImageAspectRatio; }
            set
            {
                _keepImageAspectRatio = value;
                OnPropertyChanged();
            }
        }

        public bool ReduceExactlyToResolution
        {
            get { return _reduceExactlyToResolution; }
            set
            {
                _reduceExactlyToResolution = value;
                OnPropertyChanged();
            }
        }

        public bool ReduceExactlyToLimits
        {
            get { return _reduceExactlyToLimits; }
            set
            {
                _reduceExactlyToLimits = value;
                ReduceExactlyToResolution = !_reduceExactlyToLimits;
                OnPropertyChanged();
            }
        }

        public bool ChangeOriginalCodeToMinimalistic
        {
            get { return _changeOriginalCodeToMinimalistic; }
            set
            {
                _changeOriginalCodeToMinimalistic = value;
                OnPropertyChanged();
            }
        }

        public bool UseFFMpegForCompress
        {
            get { return _useFFMpegForCompress; }
            set
            {
                _useFFMpegForCompress = value;
                OnPropertyChanged();
            }
        }

        public bool CompressLUA
        {
            get { return _compressLUA; }
            set
            {
                _compressLUA = value;
                OnPropertyChanged();
            }
        }

        public bool CompressPNG
        {
            get { return _compressPNG; }
            set
            {
                _compressPNG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressJPG
        {
            get { return _compressJPG; }
            set
            {
                _compressJPG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressVTF
        {
            get { return _compressVTF; }
            set
            {
                _compressVTF = value;
                OnPropertyChanged();
            }
        }

        public bool CompressOGG
        {
            get { return _compressOGG; }
            set
            {
                _compressOGG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressMP3
        {
            get { return _compressMP3; }
            set
            {
                _compressMP3 = value;
                OnPropertyChanged();
            }
        }

        public bool CompressWAV
        {
            get { return _compressWAV; }
            set
            {
                _compressWAV = value;
                OnPropertyChanged();
            }
        }

        public bool UnlockedUI
        {
            get { return _unlockedUI; }
            set
            {
                _unlockedUI = value;
                OnPropertyChanged();
            }
        }

        public string AddonDirectoryPath
        {
            get { return _addonDirectoryPath; }
            set
            {
                _addonDirectoryPath = value;
                OnPropertyChanged();
            }
        }

        public int ProgressBarMinValue
        {
            get { return _progressBarMinValue; }
            set
            {
                _progressBarMinValue = value;
                OnPropertyChanged();
            }
        }

        public int ProgressBarMaxValue
        {
            get { return _progressBarMaxValue; }
            set
            {
                _progressBarMaxValue = value;
                OnPropertyChanged();
            }
        }

        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged();
            }
        }

        public string ProgressBarText
        {
            get { return _progressBarText; }
            set
            {
                _progressBarText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null && propertyName != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

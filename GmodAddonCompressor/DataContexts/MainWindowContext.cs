using System.ComponentModel;
using System.Runtime.CompilerServices;
using GmodAddonCompressor.Properties;

namespace GmodAddonCompressor.DataContexts
{
    internal class MainWindowContext : INotifyPropertyChanged
    {
        private static Settings Set => Settings.Default;
        private string _addonDirectoryPath = string.Empty;
        private string _progressBarText = string.Empty;
        private int _progressBarMinValue = 0;
        private int _progressBarMaxValue = 100;
        private int _progressBarValue = 0;
        private bool _unlockedUI = Set._unlockedUI;
        private bool _compressVTF = Set._compressVTF;
        private bool _compressWAV = Set._compressWAV;
        private bool _compressMP3 = Set._compressMP3;
        private bool _compressOGG = Set._compressOGG;
        private bool _compressJPG = Set._compressJPG;
        private bool _compressPNG = Set._compressPNG;
        private bool _compressLUA = Set._compressLUA;
        private bool _useFFMpegForCompress = Set._useFFMpegForCompress;
        private bool _changeOriginalCodeToMinimalistic = Set._changeOriginalCodeToMinimalistic;
        private bool _reduceExactlyToLimits = Set._reduceExactlyToLimits;
        private bool _reduceExactlyToResolution = Set._reduceExactlyToResolution;
        private bool _keepImageAspectRatio = Set._keepImageAspectRatio;
        private bool _imageMagickVTFCompress = Set._imageMagickVTFCompress;
        private uint _imageSkipWidth = Set._imageSkipWidth;
        private uint _imageSkipHeight = Set._imageSkipHeight;
        private int _wavRate = Set._wavRate;
        private int _wavRateListIndex = Set._wavRateListIndex;
        private int _imageReducingResolutionListIndex = Set._imageReducingResolutionListIndex;
        private int _imageWidthLimitIndex = Set._imageWidthLimitIndex;
        private int _imageHeightLimitIndex = Set._imageHeightLimitIndex;
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
            44100,
            22050,
            11025
        };

        public uint ImageSkipHeight
        {
            get { return _imageSkipHeight; }
            set
            {
                _imageSkipHeight = value;
                Set._imageSkipHeight = value;
                OnPropertyChanged();
            }
        }

        public uint ImageSkipWidth
        {
            get { return _imageSkipWidth; }
            set
            {
                _imageSkipWidth = value;
                Set._imageSkipWidth = value;
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
                Set._imageWidthLimitIndex = value;
                OnPropertyChanged();
            }
        }

        public int ImageHeightLimitIndex
        {
            get { return _imageHeightLimitIndex; }
            set
            {
                _imageHeightLimitIndex = value;
                Set._imageHeightLimitIndex = value;
                OnPropertyChanged();
            }
        }

        public int ImageReducingResolutionListIndex
        {
            get { return _imageReducingResolutionListIndex; }
            set
            {
                _imageReducingResolutionListIndex = value;
                Set._imageReducingResolutionListIndex = value;
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
                Set._wavRateListIndex = value;
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
                Set._wavRate = value;
                OnPropertyChanged();
            }
        }

        public bool ImageMagickVTFCompress
        {
            get { return _imageMagickVTFCompress; }
            set
            {
                _imageMagickVTFCompress = value;
                Set._imageMagickVTFCompress = value;
                OnPropertyChanged();
            }
        }

        public bool KeepImageAspectRatio
        {
            get { return _keepImageAspectRatio; }
            set
            {
                _keepImageAspectRatio = value;
                Set._keepImageAspectRatio = value;
                OnPropertyChanged();
            }
        }

        public bool ReduceExactlyToResolution
        {
            get { return _reduceExactlyToResolution; }
            set
            {
                _reduceExactlyToResolution = value;
                Set._reduceExactlyToResolution = value;
                OnPropertyChanged();
            }
        }

        public bool ReduceExactlyToLimits
        {
            get { return _reduceExactlyToLimits; }
            set
            {
                _reduceExactlyToLimits = value;
                Set._reduceExactlyToLimits = value;
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
                Set._changeOriginalCodeToMinimalistic = value;
                OnPropertyChanged();
            }
        }

        public bool UseFFMpegForCompress
        {
            get { return _useFFMpegForCompress; }
            set
            {
                _useFFMpegForCompress = value;
                Set._useFFMpegForCompress = value;
                OnPropertyChanged();
            }
        }

        public bool CompressLUA
        {
            get { return _compressLUA; }
            set
            {
                _compressLUA = value;
                Set._compressLUA = value;
                OnPropertyChanged();
            }
        }

        public bool CompressPNG
        {
            get { return _compressPNG; }
            set
            {
                _compressPNG = value;
                Set._compressPNG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressJPG
        {
            get { return _compressJPG; }
            set
            {
                _compressJPG = value;
                Set._compressJPG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressVTF
        {
            get { return _compressVTF; }
            set
            {
                _compressVTF = value;
                Set._compressVTF = value;
                OnPropertyChanged();
            }
        }

        public bool CompressOGG
        {
            get { return _compressOGG; }
            set
            {
                _compressOGG = value;
                Set._compressOGG = value;
                OnPropertyChanged();
            }
        }

        public bool CompressMP3
        {
            get { return _compressMP3; }
            set
            {
                _compressMP3 = value;
                Set._compressMP3 = value;
                OnPropertyChanged();
            }
        }

        public bool CompressWAV
        {
            get { return _compressWAV; }
            set
            {
                _compressWAV = value;
                Set._compressWAV = value;
                OnPropertyChanged();
            }
        }

        public bool UnlockedUI
        {
            get { return _unlockedUI; }
            set
            {
                _unlockedUI = value;
                Set._unlockedUI = value;
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
                Set.Save();
        }
    }
}

using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Helpres;
using GmodAddonCompressor.Systems;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace GmodAddonCompressor
{
    public partial class MainWindow : Window
    {
        private MainWindowContext _context = new MainWindowContext();
        private const string _version = "v2.0.1";

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _context;
            VersionId.Text = _version;

            Button_Compress.Click += Button_Compress_Click;
            Button_SelectDirectory.Click += Button_SelectDirectory_Click;
        }

        private void Button_SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                _context.AddonDirectoryPath = dialog.SelectedPath;
            }
        }

        private void Button_Compress_Click(object sender, RoutedEventArgs e)
        {
            string addonDirectoryPath = _context.AddonDirectoryPath;

            if (Directory.Exists(addonDirectoryPath))
            {
                Task.Run(async () =>
                {
                    await StartCompressProcess(addonDirectoryPath);
                });
            }
        }

        private async Task StartCompressProcess(string addonDirectoryPath)
        {
            _context.UnlockedUI = false;

            await Task.Delay(500);

            int rateIndex = _context.WavRateListIndex;
            int resolutionIndex = _context.ImageReducingResolutionListIndex;
            int targetWidth = (int)_context.ImageSizeLimitList[_context.ImageWidthLimitIndex];
            int targetHeight = (int)_context.ImageSizeLimitList[_context.ImageHeightLimitIndex];

            AudioContext.RateNumber = _context.WavRateList[rateIndex];
            AudioContext.UseFFMpegForCompress = _context.UseFFMpegForCompress;
            ImageContext.Resolution = _context.ImageReducingResolutionList[resolutionIndex];
            ImageContext.TaargetWidth = targetWidth;
            ImageContext.TargetHeight = targetHeight;
            ImageContext.SkipWidth = (int)_context.ImageSkipWidth;
            ImageContext.SkipHeight = (int)_context.ImageSkipHeight;
            ImageContext.ReduceExactlyToLimits = _context.ReduceExactlyToLimits;
            ImageContext.KeepImageAspectRatio = _context.KeepImageAspectRatio;
            ImageContext.ImageMagickVTFCompress = _context.ImageMagickVTFCompress;
            LuaContext.ChangeOriginalCodeToMinimalistic = _context.ChangeOriginalCodeToMinimalistic;

            var compressSystem = new CompressAddonSystem(addonDirectoryPath);

            if (_context.CompressVTF) compressSystem.IncludeVTF();
            if (_context.CompressWAV) compressSystem.IncludeWAV();
            if (_context.CompressMP3) compressSystem.IncludeMP3();
            if (_context.CompressOGG) compressSystem.IncludeOGG();
            if (_context.CompressJPG) compressSystem.IncludeJPG();
            if (_context.CompressPNG) compressSystem.IncludePNG();
            if (_context.CompressLUA) compressSystem.IncludeLUA();

            compressSystem.e_ProgressChanged += CompressProgress;
            compressSystem.e_CompletedCompress += CompressCompleted;
            compressSystem.StartCompress();
        }

        private void CompressProgress(string filePath, int fileIndex, int filesCount)
        {
            double difference = (double)100 / (double)filesCount;
            double percent = (double)difference * (double)fileIndex;

            _context.ProgressBarMinValue = 0;
            _context.ProgressBarMaxValue = filesCount;
            _context.ProgressBarValue = fileIndex;
            _context.ProgressBarText = $"{(int)percent} % | Files: {fileIndex} / {filesCount}";
        }

        private void CompressCompleted()
        {
            _context.ProgressBarMinValue = 0;
            _context.ProgressBarMaxValue = 100;
            _context.ProgressBarValue = 0;
            _context.ProgressBarText = string.Empty;

            _context.UnlockedUI = true;
        }

        private void CheckBox_EnableDebugConsole(object sender, RoutedEventArgs e)
        {
            ConsoleHelper.AllocConsole();
        }

        private void CheckBox_DisableDebugConsole(object sender, RoutedEventArgs e)
        {
            ConsoleHelper.FreeConsole();
        }
    }
}

using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Helpres;
using GmodAddonCompressor.Systems;
using System;
using System.IO;
using System.Windows;

namespace GmodAddonCompressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowContext Context = new MainWindowContext();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = Context;

            Button_Compress.Click += Button_Compress_Click;
            Button_SelectDirectory.Click += Button_SelectDirectory_Click;
        }

        private void Button_SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                Context.AddonDirectoryPath = dialog.SelectedPath;
            }
        }

        private void Button_Compress_Click(object sender, RoutedEventArgs e)
        {
            string addonDirectoryPath = Context.AddonDirectoryPath;

            if (Directory.Exists(addonDirectoryPath))
            {
                var compressSystem = new CompressAddonSystem(addonDirectoryPath);

                if (Context.CompressVTF) compressSystem.IncludeVTF();
                if (Context.CompressWAV) compressSystem.IncludeWAV();
                //if (Context.CompressMP3) compressSystem.IncludeMP3();
                if (Context.CompressJPG) compressSystem.IncludeJPG();
                if (Context.CompressPNG) compressSystem.IncludePNG();
                if (Context.CompressLUA) compressSystem.IncludeLUA();

                int rateIndex = Context.WavRateListIndex;
                int resolutionIndex = Context.ImageReducingResolutionListIndex;

                compressSystem.SetWavRate(Context.WavRateList[rateIndex]);
                compressSystem.SetReducingResolution(Context.ImageReducingResolutionList[resolutionIndex]);

                compressSystem.e_ProgressChanged += CompressProgress;
                compressSystem.e_CompletedCompress += CompressCompleted;
                compressSystem.StartCompress();

                Context.UnlockedUI = false;
            }
        }

        private void CompressProgress(string filePath, int fileIndex, int filesCount)
        {
            double difference = (double)100 / (double)filesCount;
            double percent = (double)difference * (double)fileIndex;

            Context.ProgressBarMinValue = 0;
            Context.ProgressBarMaxValue = filesCount;
            Context.ProgressBarValue = fileIndex;
            Context.ProgressBarText = $"{(int)percent} % | Files: {fileIndex} / {filesCount}";
        }

        private void CompressCompleted()
        {
            Context.ProgressBarMinValue = 0;
            Context.ProgressBarMaxValue = 100;
            Context.ProgressBarValue = 0;
            Context.ProgressBarText = string.Empty;

            Context.UnlockedUI = true;
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

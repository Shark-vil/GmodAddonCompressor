using GmodAddonCompressor.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Systems
{
    internal class CompressAddonSystem
    {
        internal delegate void ProgressChangedEvent(string filePath, int fileIndex, int filesCount);
        internal delegate void CompletedCompressEvent();

        internal ProgressChangedEvent e_ProgressChanged;
        internal CompletedCompressEvent e_CompletedCompress;

        private VTFEdit _VTFEdit = new VTFEdit();
        private WAVEdit _WAVEdit = new WAVEdit();
        private MP3Edit _MP3Edit = new MP3Edit();
        private PNGEdit _PNGEdit = new PNGEdit();
        private JPEGEdit _JPEGEdit = new JPEGEdit();
        private JPGEdit _JPGEdit = new JPGEdit();

        private Queue<FileInfo> _registredFiles = new Queue<FileInfo>();
        private bool _hasStarted = false;
        private Thread? _compressThread = null;
        private List<string> _validFileExtensions = new List<string>();
        private string _directoryPath;

        public CompressAddonSystem(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        internal void IncludeLUA()
        {
            AddValidFileExtensions(".lua");
        }

        internal void IncludeMP3()
        {
            AddValidFileExtensions(".mp3");
        }

        internal void IncludeWAV()
        {
            AddValidFileExtensions(".wav");
        }

        internal void IncludeVTF()
        {
            AddValidFileExtensions(".vtf");
        }

        internal void IncludeJPG()
        {
            AddValidFileExtensions(".jpg");
            AddValidFileExtensions(".jpeg");
        }

        internal void IncludePNG()
        {
            AddValidFileExtensions(".png");
        }

        internal void SetWavRate(int rateNumber)
        {
            _WAVEdit.RateNumber = rateNumber;
        }

        internal void SetReducingResolution(int resolution)
        {
            _VTFEdit.Resolution = resolution;
            _JPGEdit.Resolution = resolution;
            _PNGEdit.Resolution = resolution;
        }

        internal void StartCompress()
        {
            _hasStarted = true;

            ParseDirectory(_directoryPath);

            _compressThread = new Thread(CompressThread);
            _compressThread.IsBackground = true;
            _compressThread.Priority = ThreadPriority.AboveNormal;
            _compressThread.Start();
        }

        internal void StopCompress()
        {
            if (_compressThread != null && _compressThread.IsAlive)
                _compressThread.Interrupt();
        }

        internal bool HasStarted() => _hasStarted;

        private void AddValidFileExtensions(string extension)
        {
            if (!_validFileExtensions.Exists(x => x == extension))
            {
                _validFileExtensions.Add(extension);
                Console.WriteLine("Add valid file extensions: " + extension);
            }
        }

        //private void RemoveValidFileExtensions(string extension)
        //{
        //    _validFileExtensions.RemoveAll(x => x == extension);
        //}

        private void CompressThread()
        {
            Task mainTask = Task.Run(CompressThreadAsync);
            while (mainTask.Status == TaskStatus.Running) { }
        }

        private async Task CompressThreadAsync()
        {
            int filesCount = _registredFiles.Count;
            int fileIndex = 0;

            await Parallel.ForEachAsync(_registredFiles, async (FileInfo file, CancellationToken cancellationToken) =>
            {
                string filePath = file.FullName;

                //Console.WriteLine($"Target file: {filePath.Replace(_directoryPath, string.Empty)}");

                switch (file.Extension)
                {
                    case ".vtf":
                        await _VTFEdit.VtfCompress(filePath);
                        break;

                    case ".jpg":
                        await _JPGEdit.JpgCompress(filePath);
                        break;

                    case ".jpeg":
                        await _JPEGEdit.JpegCompress(filePath);
                        break;

                    case ".png":
                        await _PNGEdit.PngCompress(filePath);
                        break;

                    case ".wav":
                        await _WAVEdit.WavCompress(filePath);
                        break;

                    case ".mp3":
                        await _MP3Edit.Mp3Compress(filePath);
                        break;

                    case ".lua":
                        using (LUAEdit luuEdit = new LUAEdit())
                        {
                            await luuEdit.LuaCompress(filePath);
                        }
                        break;
                }

                fileIndex++;

                e_ProgressChanged?.Invoke(file.FullName, fileIndex, filesCount);
            });

            _hasStarted = false;

            e_CompletedCompress?.Invoke();
        }

        private void ParseDirectory(string directoryPath)
        {
            var currentDirectory = new DirectoryInfo(directoryPath);
            FileInfo[] files = currentDirectory.GetFiles();

            foreach (FileInfo file in files)
            {
                if (_validFileExtensions.Contains(file.Extension))
                {
                    _registredFiles.Enqueue(file);
                    Console.WriteLine($"Register file: {file.FullName.Replace(_directoryPath, string.Empty)}");
                }
            }

            foreach (DirectoryInfo directory in currentDirectory.GetDirectories())
                ParseDirectory(directory.FullName);
        }
    }
}

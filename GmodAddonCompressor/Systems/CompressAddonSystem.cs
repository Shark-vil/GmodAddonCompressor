using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Objects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Systems
{
    internal class CompressAddonSystem
    {
        internal delegate void ProgressChangedEvent(string filePath, int fileIndex, int filesCount);
        internal delegate void CompletedCompressEvent();

        internal ProgressChangedEvent? e_ProgressChanged;
        internal CompletedCompressEvent? e_CompletedCompress;

        private Queue<FileInfo> _registredFiles = new Queue<FileInfo>();
        private bool _hasStarted = false;
        private Thread? _compressThread = null;
        private List<string> _validFileExtensions = new List<string>();
        private string _directoryPath;
        private Dictionary<string, ICompress> _compressServices = new Dictionary<string, ICompress>();
        private readonly ILogger _logger = LogSystem.CreateLogger<CompressAddonSystem>();

        public CompressAddonSystem(string directoryPath)
        {
            _directoryPath = directoryPath;
            CompressDirectoryContext.DirectoryPath = _directoryPath;
        }

        internal void IncludeLUA()
        {
            string extension = ".lua";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new LUAEdit());
        }

        internal void IncludeOGG()
        {
            string extension = ".ogg";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new OGGEdit());
        }

        internal void IncludeMP3()
        {
            string extension = ".mp3";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new MP3Edit());
        }

        internal void IncludeWAV()
        {
            string extension = ".wav";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new WAVEdit());
        }

        internal void IncludeVTF()
        {
            string extension = ".vtf";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new VTFEdit());
        }

        internal void IncludeJPG()
        {
            string extension = ".jpg";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new JPGEdit());

            extension = ".jpeg";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new JPEGEdit());
        }

        internal void IncludePNG()
        {
            string extension = ".png";
            AddValidFileExtensions(extension);
            _compressServices.Add(extension, new PNGEdit());
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
                _validFileExtensions.Add(extension);
        }

        private ICompress? GetService(string extension)
        {
            if (_compressServices.TryGetValue(extension, out var service))
                return service;
            return null;
        }

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
                ICompress? service = GetService(file.Extension);

                if (service != null)
                    await service.Compress(file.FullName);

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
                    _registredFiles.Enqueue(file);
            }

            foreach (DirectoryInfo directory in currentDirectory.GetDirectories())
                ParseDirectory(directory.FullName);
        }
    }
}

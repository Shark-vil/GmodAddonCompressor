using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPEGEdit : ImageEditBase, ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<JPEGEdit>();

        public JPEGEdit()
        {
            SetImageFileExtension(".jpeg");
        }

        public async Task Compress(string jpegFilePath)
        {
            try
            {
                await Task.WhenAny(ImageCompress(jpegFilePath), Task.Delay(TimeSpan.FromMinutes(2)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}

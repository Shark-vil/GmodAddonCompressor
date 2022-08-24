using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPGEdit : ImageEditBase, ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<JPGEdit>();

        public JPGEdit()
        {
            SetImageFileExtension(".jpg");
        }

        public async Task Compress(string jpgFilePath)
        {
            try
            {
                await Task.WhenAny(ImageCompress(jpgFilePath), Task.Delay(TimeSpan.FromMinutes(2)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}

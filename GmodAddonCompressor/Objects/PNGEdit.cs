using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class PNGEdit : ImageEditBase, ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<PNGEdit>();

        public PNGEdit()
        {
            SetImageFileExtension(".png");
        }

        public async Task Compress(string pngFilePath)
        {
            try
            {
                await Task.WhenAny(ImageCompress(pngFilePath), Task.Delay(TimeSpan.FromMinutes(2)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}

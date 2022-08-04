using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Systems;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using GmodAddonCompressor.CustomExtensions;

namespace GmodAddonCompressor.Bases
{
    internal abstract class ImageEditBase
    {
        protected string _fileExtension = string.Empty;
        private readonly ILogger _logger = LogSystem.CreateLogger<ImageEditBase>();

        protected async Task ImageCompress(string imageFilePath)
        {
            if (string.IsNullOrEmpty(_fileExtension))
                throw new Exception("Not set image file extension");

            string tempImageFilePath = imageFilePath + "____TEMP." + _fileExtension;

            if (!File.Exists(tempImageFilePath))
                File.Copy(imageFilePath, tempImageFilePath);

            if (File.Exists(imageFilePath))
                File.Delete(imageFilePath);

            int currentWidth = 0;
            int currentHeight = 0;

            using (FileStream fs = new FileStream(tempImageFilePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    try
                    {
                        Bitmap original = (Bitmap)image;

                        currentWidth = original.Width;
                        currentHeight = original.Height;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            uint skipWidth = ImageContext.SkipWidth;
            uint skipHeight = ImageContext.SkipHeight;

            if (
                currentWidth != 0 && currentHeight != 0
                && (skipWidth == 0 || currentWidth > skipWidth)
                && (skipHeight == 0 || currentHeight > skipHeight)
            )
            {
                int resolution = ImageContext.Resolution;
                int newWidth = currentWidth / resolution;
                int newHeight = currentHeight / resolution;

                await SaveMagickImage(tempImageFilePath, imageFilePath, newWidth, newHeight);
            }

            if (File.Exists(tempImageFilePath))
            {
                if (File.Exists(imageFilePath))
                {
                    long oldFileSize = new FileInfo(tempImageFilePath).Length;
                    long newFileSize = new FileInfo(imageFilePath).Length;

                    if (newFileSize > oldFileSize)
                    {
                        File.Delete(imageFilePath);
                        File.Copy(tempImageFilePath, imageFilePath);

                        _logger.LogError($"Image compression failed: {imageFilePath.GAC_ToLocalPath()}");
                    }
                    else
                        _logger.LogInformation($"Successful file compression: {imageFilePath.GAC_ToLocalPath()}");
                }
                else
                    File.Copy(tempImageFilePath, imageFilePath);

                File.Delete(tempImageFilePath);
            }
        }

        protected void SetImageFileExtension(string fileExtension)
        {
            _fileExtension = fileExtension;
        }

        private async Task SaveMagickImage(string imageSourcePath, string imageSavePath, int newWidth, int newHeight)
        {
            int resizeWidth = newWidth;
            int resizeHeight = newHeight;
            uint minimumSizeLimit = ImageContext.MinimumSizeLimit;

            if (newWidth < minimumSizeLimit || newHeight < minimumSizeLimit)
            {
                resizeWidth = (int)minimumSizeLimit;
                resizeHeight = (int)minimumSizeLimit;
            }

            using (var image = new MagickImage(imageSourcePath))
            {
                try
                {
                    var size = new MagickGeometry(resizeWidth, resizeHeight);
                    size.IgnoreAspectRatio = false;

                    image.Resize(size);
                    image.SetCompression(CompressionMethod.LZMA);
                    image.Write(imageSavePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            long oldFileSize = -1;
            DateTime timeOut = DateTime.UtcNow.AddSeconds(5);

            while (oldFileSize == -1 && DateTime.UtcNow > timeOut)
            {
                try
                {
                    oldFileSize = new FileInfo(imageSavePath).Length;
                }
                catch
                {
                    await Task.Yield();
                }
            }

            string additionalCompressionFilePath = imageSavePath + "____TEMPCOMPRESS." + _fileExtension;
            File.Copy(imageSavePath, additionalCompressionFilePath);

            try
            {
                FileInfo file = new FileInfo(additionalCompressionFilePath);

                var optimizer = new ImageOptimizer();
                optimizer.LosslessCompress(file);

                file.Refresh();

                if (file.Length > oldFileSize)
                    File.Delete(additionalCompressionFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            if (File.Exists(additionalCompressionFilePath))
            {
                File.Delete(imageSavePath);
                File.Copy(additionalCompressionFilePath, imageSavePath);
            }
        }
    }
}

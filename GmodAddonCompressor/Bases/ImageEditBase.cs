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

        protected int[] GetImageSize(string imageFilePath)
        {
            int width = 0;
            int height = 0;

            using (FileStream fs = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    try
                    {
                        Bitmap original = (Bitmap)image;

                        width = original.Width;
                        height = original.Height;
                    }
                    catch (Exception ex)
                    {
                         _logger.LogError(ex.ToString());
                    }
                }
            }

            return new int[]
            {
                width,
                height,
            };
        }

        protected int[] GetReduceResolutionSize(int width, int height)
        {
            int skipWidth = ImageContext.SkipWidth;
            int skipHeight = ImageContext.SkipHeight;

            int newWidth = 0;
            int newHeight = 0;

            if (
                width != 0 && height != 0
                && (skipWidth == 0 || width > skipWidth)
                && (skipHeight == 0 || height > skipHeight)
            )
            {
                if (ImageContext.ReduceExactlyToLimits)
                {
                    newWidth = ImageContext.TaargetWidth;
                    newHeight = ImageContext.TargetHeight;
                }
                else
                {
                    int resolution = ImageContext.Resolution;
                    newWidth = FloorPowerTwo(width / resolution);
                    newHeight = FloorPowerTwo(height / resolution);
                }
            }

            return new int[]
            {
                newWidth,
                newHeight,
            };
        }

        protected bool ImageIsSingleColor(string imageFilePath)
        {
            IMagickColor<ushort>? firstColorPixel = null;
            bool isFindedColor = false;
            bool isSingleColor = true;

            using (var image = new MagickImage(imageFilePath))
            {
                using (IPixelCollection<ushort> pixels = image.GetPixels())
                {
                    try
                    {
                        for (int xPixel = 0; xPixel < image.Width; xPixel++)
                        {
                            for (int yPixel = 0; yPixel < image.Height; yPixel++)
                            {
                                IPixel<ushort> getPixel = pixels.GetPixel(xPixel, yPixel);
                                IMagickColor<ushort>? getColor = getPixel.ToColor();

                                if (!isFindedColor)
                                {
                                    firstColorPixel = getColor;
                                    isFindedColor = true;
                                }
                                else if (firstColorPixel == null || getColor == null || !firstColorPixel.Equals(getColor))
                                {
                                    isSingleColor = false;
                                    break;
                                }
                            }

                            if (!isSingleColor) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }
            }

            /*
            using (FileStream fs = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    try
                    {
                        Bitmap original = (Bitmap)image;

                        for (int xPixel = 0; xPixel < original.Width; xPixel++)
                        {
                            for (int yPixel = 0; yPixel < original.Height; yPixel++)
                            {
                                Color getColor = original.GetPixel(xPixel, yPixel);

                                if (!isFindedColor)
                                {
                                    firstColorPixel = getColor.ToArgb();
                                    Console.WriteLine($"Pxl : {firstColorPixel}");
                                    isFindedColor = true;
                                }
                                else if (firstColorPixel != getColor.ToArgb())
                                {
                                    Console.WriteLine($"Pxl wrong : {firstColorPixel} != {getColor.ToArgb()}");
                                    isSingleColor = false;
                                    break;
                                }
                            }

                            if (!isSingleColor) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            */

            return isSingleColor;
        }

        protected bool ImageIsFullTransparent(string imageFilePath)
        {
            bool isTransparent = true;

            using (FileStream fs = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    try
                    {
                        Bitmap original = (Bitmap)image;
    
                        for (int xPixel = 0; xPixel < original.Width; xPixel++)
                        {
                            for (int yPixel = 0; yPixel < original.Height; yPixel++)
                            {
                                if (original.GetPixel(xPixel, yPixel).A > 0)
                                {
                                    isTransparent = false;
                                    break;
                                }
                            }

                            if (!isTransparent) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            return isTransparent;
        }

        protected async Task ImageCompress(string imageFilePath)
        {
            if (string.IsNullOrEmpty(_fileExtension))
                throw new Exception("Not set image file extension");

            string tempImageFilePath = imageFilePath + "____TEMP" + _fileExtension;

            if (File.Exists(tempImageFilePath))
                File.Delete(tempImageFilePath);

            await SaveMagickImage(tempImageFilePath, imageFilePath);

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

        private int FloorPowerTwo(int x)
        {
            if (x < 1) return 1;
            return (int)System.Math.Pow(2, (int)System.Math.Log(x, 2));
        }

        private async Task SaveMagickImage(string imageSourcePath, string imageSavePath)
        {
            int[] imageSize = GetImageSize(imageSavePath);
            if (imageSize[0] == 0 || imageSize[1] == 0) return;

            int[] newImageSize = GetReduceResolutionSize(imageSize[0], imageSize[1]);

            int newWidth = newImageSize[0];
            int newHeight = newImageSize[1];

            bool isSingleColor = ImageIsSingleColor(imageSavePath);
            
            int resizeWidth = isSingleColor ? 1 : (newWidth < ImageContext.TaargetWidth ? ImageContext.TaargetWidth : newWidth);
            int resizeHeight = isSingleColor ? 1 :(newHeight < ImageContext.TargetHeight ? ImageContext.TargetHeight : newHeight);

            if (newWidth > imageSize[0] || newHeight > imageSize[1]) return;

            if (!File.Exists(imageSourcePath))
                File.Copy(imageSavePath, imageSourcePath);

            if (File.Exists(imageSavePath))
                File.Delete(imageSavePath);

            using (var image = new MagickImage(imageSourcePath))
            {
                try
                {
                    if (ImageContext.RemoveRedundantAlpha)
                        await RemoveRedundantAlpha(image);

                    var size = new MagickGeometry(resizeWidth, resizeHeight);
                    size.IgnoreAspectRatio = isSingleColor ? true : !ImageContext.KeepImageAspectRatio;

                    image.Resize(size);

                    if (!isSingleColor)
                        image.SetCompression(CompressionMethod.LZMA);

                    image.Write(imageSavePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            if (isSingleColor)
                return;

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

            string additionalCompressionFilePath = imageSavePath + "____TEMPCOMPRESS" + _fileExtension;
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

        /*
         * The Following checks if an alpha is redundant by the alpha's deviation
         * A Value of 0 or NaN indicates a solid, unused alpha
         * Alpha is then disabled: drastically reducing filesize
         */
        private async Task RemoveRedundantAlpha(MagickImage image)
        {
            _logger.LogInformation($"Attempting to remove redundant alphas..");
            try
            {
                if (!image.HasAlpha)
                    return;

                double dev = GetStandardDeviation(image, PixelChannel.Alpha);
                if (dev == 0 || Double.IsNaN(dev))
                {
                    image.Alpha(AlphaOption.Off);
                }
            }
            catch (Exception ex)
            {
                if(ex==null)
                    _logger.LogError("Something went wrong removing redundant alphas");
                else
                    _logger.LogError(ex.ToString());
            }

        }

        private double GetStandardDeviation(MagickImage image, PixelChannel channel)
        {
            // Get the statistics of the alpha channel
            var statistics = image.Statistics();

            var alphaStatistics = statistics.GetChannel(PixelChannel.Alpha);

            // The StandardDeviation property gives you the standard deviation
            if (alphaStatistics == null)
                return -1;
            else
                return alphaStatistics.StandardDeviation;
        }
    }


}



using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Bases
{
    internal abstract class ImageEditBase
    {
        // 64, 128, 256, 512
        private static uint _minimumSizeLimit = 256;
        private static uint _skipWidth = 0;
        private static uint _skipHeight = 0;

        protected int _resolution = 4;
        protected string _fileExtension = string.Empty;

        internal int Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value < 2 ? 2 : value > 6 ? 6 : value;
            }
        }

        internal static void SetMinimumSizeLimit(uint sizeLimit)
        {
            _minimumSizeLimit = sizeLimit;
        }

        internal static void SetSkipSizeLimit(uint width, uint height)
        {
            _skipWidth = width;
            _skipHeight = height;
        }

        protected async Task ImageCompress(string imageFilePath)
        {
            if (string.IsNullOrEmpty(_fileExtension))
                throw new Exception("Not set image file extension");

            string tempImageFilePath = imageFilePath + "_temp." + _fileExtension;

            if (!File.Exists(tempImageFilePath))
                File.Copy(imageFilePath, tempImageFilePath);

            await Task.Yield();

            if (File.Exists(imageFilePath))
                File.Delete(imageFilePath);

            await Task.Yield();

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

            if (
                currentWidth != 0 && currentHeight != 0
                && (_skipWidth == 0 || currentWidth > _skipWidth)
                && (_skipHeight == 0 || currentHeight > _skipHeight)
            )
            {
                await Task.Yield();

                int newWidth = currentWidth / _resolution;
                int newHeight = currentHeight / _resolution;

                SaveBitmap(tempImageFilePath, imageFilePath, newWidth, newHeight);
            }

            await Task.Yield();

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

                        Console.WriteLine($"Image compression failed: {imageFilePath}");
                    }
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

        protected void SaveBitmap(string imageSourcePath, string imageSavePath, int newWidth, int newHeight)
        {
            if (newWidth < _minimumSizeLimit || newHeight < _minimumSizeLimit)
            {
                newWidth = (int)_minimumSizeLimit;
                newHeight = (int)_minimumSizeLimit;
            }

            using (var image = new MagickImage(imageSourcePath))
            {
                var size = new MagickGeometry(newWidth, newHeight);
                size.IgnoreAspectRatio = false;

                image.Resize(size);

                //if (_fileExtension == "jpg" || _fileExtension == "jpeg")
                //    image.SetCompression(CompressionMethod.JPEG);
                //else
                //    image.SetCompression(CompressionMethod.LZMA);

                image.SetCompression(CompressionMethod.LZMA);
                image.Write(imageSavePath);
            }

            //try
            //{
            //    FileInfo file = new FileInfo(imageSavePath);

            //    var optimizer = new ImageOptimizer();
            //    optimizer.LosslessCompress(file);

            //    file.Refresh();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
        }
    }
}

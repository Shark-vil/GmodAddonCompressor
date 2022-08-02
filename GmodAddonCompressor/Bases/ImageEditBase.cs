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
        // 64, 256, 512
        private const int _minimumSizeLimit = 256;

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

                        //int newWidth = currentWidth / _resolution;
                        //int newHeight = currentHeight / _resolution;

                        /*
                        if (!SaveBitmap(imageFilePath, original, newWidth, newHeight) && _resolution != 2)
                        {
                            SaveBitmap(imageFilePath, original, currentWidth / 2, currentHeight / 2);
                        }
                        */

                        //if (!SaveBitmap(tempImageFilePath, imageFilePath, newWidth, newHeight) && _resolution != 2)
                        //{
                        //    SaveBitmap(tempImageFilePath, imageFilePath, currentWidth / 2, currentHeight / 2);
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            if (currentWidth != 0 && currentHeight != 0)
            {
                await Task.Yield();

                int newWidth = currentWidth / _resolution;
                int newHeight = currentHeight / _resolution;

                if (!SaveBitmap(tempImageFilePath, imageFilePath, newWidth, newHeight) && _resolution != 2)
                {
                    SaveBitmap(tempImageFilePath, imageFilePath, currentWidth / 2, currentHeight / 2);
                }
            }

            await Task.Yield();

            if (File.Exists(tempImageFilePath))
            {
                if (!File.Exists(imageFilePath))
                    File.Copy(tempImageFilePath, imageFilePath);
                /*
                else
                {
                    Console.WriteLine($"Compress image: {imageFilePath}");

                    try
                    {
                        var file = new FileInfo(imageFilePath);

                        var optimizer = new ImageOptimizer();
                        optimizer.Compress(file);

                        file.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                */

                File.Delete(tempImageFilePath);
            }
        }

        protected void SetImageFileExtension(string fileExtension)
        {
            _fileExtension = fileExtension;
        }

        protected bool SaveBitmap(string imageSourcePath, string imageSavePath, int newWidth, int newHeight)
        {
            if (newWidth >= _minimumSizeLimit && newHeight >= _minimumSizeLimit)
            {
                using (var image = new MagickImage(imageSourcePath))
                {
                    var size = new MagickGeometry(newWidth, newHeight);
                    size.IgnoreAspectRatio = true;

                    image.Resize(size);
                    image.SetCompression(CompressionMethod.LZMA);
                    image.Write(imageSavePath);
                }

                return true;
            }

            return false;
        }

        /*
        protected bool SaveBitmap(string imageFilePath, Bitmap original, int newWidth, int newHeight)
        {
            if (newWidth >= _minimumSizeLimit && newHeight >= _minimumSizeLimit)
            {
                Size imageSize = new Size(newWidth, newHeight);

                using (var resized = new Bitmap(original, imageSize))
                {
                    resized.Save(imageFilePath);

                    Console.WriteLine($"Resize image: {imageFilePath}");
                }

                return true;
            }

            return false;
        }
        */
    }
}

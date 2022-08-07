using GmodAddonCompressor.Bases;
using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Properties;
using GmodAddonCompressor.Systems;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class VTFEdit : ImageEditBase, ICompress
    {
        private const string _mainDirectoryName = "VTFEdit";
        private readonly string _vtfCmdFilePath;
        private string _mainDirectoryPath;
        private readonly ILogger _logger = LogSystem.CreateLogger<VTFEdit>();

        public VTFEdit()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _mainDirectoryPath = Path.Combine(baseDirectory, _mainDirectoryName);

            if (!Directory.Exists(_mainDirectoryPath))
            {
                string zipResourcePath = Path.Combine(baseDirectory, _mainDirectoryName + ".zip");

                if (!File.Exists(zipResourcePath))
                    File.WriteAllBytes(zipResourcePath, Resources.VTFEdit);

                ZipFile.ExtractToDirectory(zipResourcePath, baseDirectory);
                File.Delete(zipResourcePath);
            }

            _vtfCmdFilePath = Path.Combine(_mainDirectoryPath, "VTFCmd.exe");

            SetImageFileExtension(".png");
        }

        public async Task Compress(string vtfFilePath)
        {
            string pngFilePath = vtfFilePath.Substring(0, vtfFilePath.Length - 3);
            pngFilePath += "png";

            long oldFileSize = new FileInfo(vtfFilePath).Length;
            long newFileSize = 0;

            await VtfToPng(vtfFilePath);

            if (File.Exists(pngFilePath))
            {
                string tempVtfFilePath = vtfFilePath + "____TEMP.vtf";

                File.Copy(vtfFilePath, tempVtfFilePath);
                File.Delete(vtfFilePath);

                try
                {
                    if (!ImageContext.ImageMagickVTFCompress)
                    {
                        await OptImageToVtf(pngFilePath);
                    }
                    else
                    {
                        await OptImageAndExportToVtf(pngFilePath);

                        if (File.Exists(vtfFilePath))
                        {
                            newFileSize = new FileInfo(vtfFilePath).Length;
                            if (newFileSize >= oldFileSize)
                            {
                                File.Delete(vtfFilePath);
                                newFileSize = 0;
                            }
                        }

                        if (!File.Exists(vtfFilePath))
                            await OptImageToVtf(pngFilePath);
                    }

                    if (File.Exists(vtfFilePath))
                    {
                        newFileSize = newFileSize != 0 ? newFileSize : new FileInfo(vtfFilePath).Length;
                        if (newFileSize < oldFileSize)
                        {
                            if (File.Exists(tempVtfFilePath)) File.Delete(tempVtfFilePath);
                            _logger.LogInformation($"Successful file compression: {vtfFilePath.GAC_ToLocalPath()}");
                        }
                    }

                    if (File.Exists(tempVtfFilePath))
                    {
                        if (File.Exists(vtfFilePath)) File.Delete(vtfFilePath);

                        File.Copy(tempVtfFilePath, vtfFilePath);
                        File.Delete(tempVtfFilePath);

                        _logger.LogError($"VTF compression failed: {vtfFilePath.GAC_ToLocalPath()}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                File.Delete(pngFilePath);
            }
        }

        private async Task OptImageAndExportToVtf(string pngFilePath)
        {
            try
            {
                bool isTransparent = ImageIsFullTransparent(pngFilePath);
                if (!isTransparent)
                {
                    await ImageCompress(pngFilePath);
                    await ImageToVtf(pngFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private async Task StartVtfCmdProcess(string arguments)
        {
            var vtfCmdProcess = new Process();
            vtfCmdProcess.StartInfo.FileName = _vtfCmdFilePath;
            vtfCmdProcess.StartInfo.Arguments = arguments;
            vtfCmdProcess.StartInfo.UseShellExecute = false;
            vtfCmdProcess.StartInfo.CreateNoWindow = true;
            vtfCmdProcess.Start();

            await vtfCmdProcess.WaitForExitAsync();
        }

        private async Task ChangeVTFVersionTo_7_4(string vtfFilePath)
        {
            using (FileStream FS = File.OpenRead(vtfFilePath))
            {
                using (BinaryReader BR = new BinaryReader(FS))
                {
                    try
                    {
                        int id = BR.ReadInt32();
                        if (id != 0x465456)
                            return;

                        int majorVersion = BR.ReadInt32();
                        int minorVersion = BR.ReadInt32();
                        if (minorVersion != 5) return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        return;
                    }
                }
            }

            await Task.Yield();

            using (FileStream FS = File.OpenWrite(vtfFilePath))
            {
                using (BinaryWriter BW = new BinaryWriter(FS))
                {
                    try
                    {
                        BW.Seek(8, SeekOrigin.Begin);
                        BW.Write((int)4);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }
            }
        }

        private async Task VtfToPng(string vtfFilePath, string vtfDirectory = "")
        {
            await VtfToImage("png", vtfFilePath, vtfDirectory);
        }

        private async Task OptImageToVtf(string imageFilePath, string? pngDirectory = null)
        {
            if (string.IsNullOrEmpty(pngDirectory))
                pngDirectory = Path.GetDirectoryName(imageFilePath);

            int[] imageSize = GetImageSize(imageFilePath);
            int imageWidth = imageSize[0];
            int imageHeight = imageSize[1];

            if (imageWidth == 0 || imageWidth == 0) return;

            int[] newImageSize = GetReduceResolutionSize(imageWidth, imageHeight);
            int newWidth = newImageSize[0];
            int newHeight = newImageSize[1];

            if (newWidth == 0 || newHeight == 0) return;

            bool isSingleColor = ImageIsSingleColor(imageFilePath);

            newWidth = isSingleColor ? 1 : (newWidth < ImageContext.TaargetWidth ? ImageContext.TaargetWidth : newWidth);
            newHeight = isSingleColor ? 1 : (newHeight < ImageContext.TargetHeight ? ImageContext.TargetHeight : newHeight);

            if (newWidth > imageWidth || newHeight > imageHeight) return;

            if (!isSingleColor && ImageContext.KeepImageAspectRatio)
            {
                try
                {
                    using (var image = new MagickImage(imageFilePath))
                    {
                        try
                        {
                            var size = new MagickGeometry(newWidth, newHeight);
                            size.IgnoreAspectRatio = false;

                            image.Resize(size);

                            imageWidth = image.Width;
                            imageHeight = image.Height;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            string arguments = string.Empty;
            arguments += $" -file \"{imageFilePath}\"";
            arguments += $" -output \"{pngDirectory}\"";
            arguments += $" -resize -rwidth {imageWidth} -rheight {imageHeight}";

            await StartVtfCmdProcess(arguments);
        }

        private async Task ImageToVtf(string imageFilePath, string? pngDirectory = null)
        {
            if (string.IsNullOrEmpty(pngDirectory))
                pngDirectory = Path.GetDirectoryName(imageFilePath);

            string arguments = string.Empty;
            arguments += $" -file \"{imageFilePath}\"";
            arguments += $" -output \"{pngDirectory}\"";

            await StartVtfCmdProcess(arguments);
        }

        private async Task VtfToImage(string fileExtension, string vtfFilePath, string? vtfDirectory = null)
        {
            if (string.IsNullOrEmpty(vtfDirectory))
                vtfDirectory = Path.GetDirectoryName(vtfFilePath);

            string arguments = string.Empty;
            arguments += $" -file \"{vtfFilePath}\"";
            arguments += $" -output \"{vtfDirectory}\"";
            arguments += $" -exportformat \"{fileExtension}\"";

            await ChangeVTFVersionTo_7_4(vtfFilePath);
            await StartVtfCmdProcess(arguments);
        }
    }
}

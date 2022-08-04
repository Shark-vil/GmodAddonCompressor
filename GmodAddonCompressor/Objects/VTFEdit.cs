using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Properties;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class VTFEdit : PNGEdit, ICompress
    {
        private const string _mainDirectoryName = "VTFEdit";
        private readonly string _vtfCmdFilePath;
        private string _mainDirectoryPath;
        private readonly ILogger _logger = LogSystem.CreateLogger<VTFEdit>();

        public VTFEdit() : base()
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
        }

        new public async Task Compress(string vtfFilePath)
        {
            long oldFileSize = -1;
            long newFileSize = -1;

            string pngFilePath = vtfFilePath.Substring(0, vtfFilePath.Length - 3);
            pngFilePath += "png";

            await VtfToPng(vtfFilePath);

            if (File.Exists(pngFilePath))
            {
                try
                {
                    oldFileSize = new FileInfo(pngFilePath).Length;

                    await base.Compress(pngFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                if (File.Exists(pngFilePath))
                {
                    try
                    {
                        if (oldFileSize != -1)
                        {
                            newFileSize = new FileInfo(pngFilePath).Length;

                            if (newFileSize < oldFileSize)
                            {
                                await ImageToVtf(pngFilePath);

                                _logger.LogInformation($"Successful file compression: {vtfFilePath.GAC_ToLocalPath()}");
                            }
                            else
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

        //private async Task VtfToJpg(string vtfFilePath, string vtfDirectory = "")
        //{
        //    await VtfToImage("jpg", vtfFilePath, vtfDirectory);
        //}

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

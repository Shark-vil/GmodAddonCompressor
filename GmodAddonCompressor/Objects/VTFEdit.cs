using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class VTFEdit : PNGEdit
    {
        private const string _mainDirectoryName = "VTFEdit";
        private readonly string _vtfCmdFilePath;
        private string _mainDirectoryPath;

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

        internal async Task VtfCompress(string vtfFilePath)
        {
            string pngFilePath = vtfFilePath.Substring(0, vtfFilePath.Length - 3);
            pngFilePath += "png";

            await VtfToPng(vtfFilePath);

            if (File.Exists(pngFilePath))
            {
                long oldFileSize = new FileInfo(pngFilePath).Length;

                await PngCompress(pngFilePath);

                if (File.Exists(pngFilePath))
                {
                    long newFileSize = new FileInfo(pngFilePath).Length;

                    if (newFileSize < oldFileSize)
                        await ImageToVtf(pngFilePath);
                    else
                        Console.WriteLine($"VTF compression failed: {vtfFilePath}");

                    File.Delete(pngFilePath);
                }
            }
        }

        internal async Task ChangeVTFVersionTo_7_4(string vtfFilePath)
        {
            using (FileStream FS = File.OpenRead(vtfFilePath))
            {
                using (BinaryReader BR = new BinaryReader(FS))
                {
                    int id = BR.ReadInt32();
                    if (id != 0x465456)
                        return;

                    int majorVersion = BR.ReadInt32();
                    int minorVersion = BR.ReadInt32();
                    if (minorVersion != 5) return;
                }
            }

            await Task.Yield();

            using (FileStream FS = File.OpenWrite(vtfFilePath))
            {
                using (BinaryWriter BW = new BinaryWriter(FS))
                {
                    BW.Seek(8, SeekOrigin.Begin);
                    BW.Write((int)4);
                }
            }
        }

        internal async Task VtfToPng(string vtfFilePath, string vtfDirectory = "")
        {
            await VtfToImage("png", vtfFilePath, vtfDirectory);
        }

        //internal async Task VtfToJpg(string vtfFilePath, string vtfDirectory = "")
        //{
        //    await VtfToImage("jpg", vtfFilePath, vtfDirectory);
        //}

        internal async Task ImageToVtf(string imageFilePath, string pngDirectory = "")
        {
            if (string.IsNullOrEmpty(pngDirectory))
                pngDirectory = Path.GetDirectoryName(imageFilePath);

            //string imageExtension = Path.GetExtension(imageFilePath);
            //imageExtension = imageExtension.Substring(1);

            //Console.WriteLine($"{imageExtension.ToLower()} to VTF: {imageFilePath}");

            string arguments = string.Empty;
            arguments += $" -file \"{imageFilePath}\"";
            arguments += $" -output \"{pngDirectory}\"";

            await StartVtfCmdProcess(arguments);
        }

        internal async Task VtfToImage(string fileExtension, string vtfFilePath, string vtfDirectory = "")
        {
            if (string.IsNullOrEmpty(vtfDirectory))
                vtfDirectory = Path.GetDirectoryName(vtfFilePath);

            //Console.WriteLine($"VTF to {fileExtension.ToLower()}: {vtfFilePath}");

            string arguments = string.Empty;
            arguments += $" -file \"{vtfFilePath}\"";
            arguments += $" -output \"{vtfDirectory}\"";
            arguments += $" -exportformat \"{fileExtension}\"";

            await ChangeVTFVersionTo_7_4(vtfFilePath);
            await StartVtfCmdProcess(arguments);
        }
    }
}

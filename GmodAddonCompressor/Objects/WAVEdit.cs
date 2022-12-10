using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class WAVEdit : ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<WAVEdit>();

        public async Task Compress(string wavFilePath)
        {
            string newWavFilePath = wavFilePath + "____TEMP.wav";

            if (File.Exists(newWavFilePath))
                File.Delete(newWavFilePath);

            try
            {
                bool wavIsLooping = false;

                using (var fs = new FileStream(wavFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    while (fs.Position < fs.Length)
                    {
                        long chunkStartPos = fs.Position;
                        string chunkId = ReadBytesToString(fs, 4);
                        uint chunkSize = ReadBytesToInt(fs, 4);
                        long chunkEndPos = chunkStartPos + chunkSize + 8;

                        switch (chunkId.ToUpper().Trim())
                        {
                            case "RIFF":
                                ReadBytes(fs, 4);
                                break;
                            case "SMPL":
                                wavIsLooping = true;
                                break;
                            case "CUE":
                                wavIsLooping = true;
                                break;
                            default:
                                fs.Position = chunkEndPos;
                                break;
                        }

                        if (wavIsLooping)
                            break;
                    }
                }

                if (wavIsLooping)
                {
                    _logger.LogWarning($"Converting of looped files is not supported now! ( {wavFilePath} )");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            using (var reader = new WaveFileReader(wavFilePath))
            {
                WaveFormat currentFormet = reader.WaveFormat;
                int rateNumber = AudioContext.SamplingFrequency;

                if (currentFormet.SampleRate <= rateNumber)
                    return;

                var newFormat = new WaveFormat(rateNumber, 16, 1);

                try
                {
                    using (var c = new WaveFormatConversionStream(newFormat, reader))
                    {
                        WaveFileWriter.CreateWaveFile(newWavFilePath, c);
                    }
                }
                catch (NAudio.MmException ex)
                {
                    if (ex.Result == NAudio.MmResult.AcmNotPossible)
                        _logger.LogError($"{wavFilePath.GAC_ToLocalPath()}\n" +
                            "WAV file conversion error! " +
                            "The required codec may not be installed on the computer: " +
                            $"{reader.WaveFormat.Encoding}\n{ex}");
                    else
                        _logger.LogError(ex.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            await Task.Yield();


            try
            {
                bool hasCompress = false;

                if (File.Exists(newWavFilePath))
                {
                    long oldFileSize = new FileInfo(wavFilePath).Length;
                    long newFileSize = new FileInfo(newWavFilePath).Length;

                    if (newFileSize < oldFileSize)
                    {
                        File.Delete(wavFilePath);
                        File.Copy(newWavFilePath, wavFilePath);
                        hasCompress = true;
                    }
                }

                if (!hasCompress && AudioContext.UseFFMpegForCompress)
                    hasCompress = await new FFMpegSystem().CompressAudioAsync(wavFilePath, newWavFilePath, AudioContext.SamplingFrequency);

                if (hasCompress)
                    _logger.LogInformation($"Successful file compression: {wavFilePath.GAC_ToLocalPath()}");
                else
                    _logger.LogError($"WAV compression failed: {wavFilePath.GAC_ToLocalPath()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            if (File.Exists(newWavFilePath))
                File.Delete(newWavFilePath);
        }

        private uint ReadBytesToInt(FileStream fs, int byteNum)
        {
            return BitConverter.ToUInt32(ReadBytes(fs, byteNum), 0);
        }

        private string ReadBytesToString(FileStream fs, int byteNum)
        {
            return System.Text.Encoding.UTF8.GetString(ReadBytes(fs, byteNum)).Trim('\0');
        }

        private byte[] ReadBytes(FileStream fs, int num)
        {
            byte[] bytes = new byte[num < 4 ? 4 : num];
            for (int i = 0; i < num; i++)
            {
                int b = fs.ReadByte();
                bytes[i] = (byte)b;
            }
            return bytes;
        }
    }
}

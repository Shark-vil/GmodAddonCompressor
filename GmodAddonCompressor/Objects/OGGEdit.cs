using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class OGGEdit : ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<OGGEdit>();

        public async Task Compress(string oggFilePath)
        {
            string newOggFilePath = oggFilePath + "____TEMP.ogg";

            using (var reader = new VorbisWaveReader(oggFilePath))
            {
                WaveFormat currentFormet = reader.WaveFormat;
                int rateNumber = AudioContext.RateNumber;

                if (currentFormet.SampleRate > rateNumber)
                {
                    var newFormat = new WaveFormat(rateNumber, 16, 1);

                    try
                    {
                        using (var c = new WaveFormatConversionStream(newFormat, reader))
                        {
                            WaveFileWriter.CreateWaveFile(newOggFilePath, c);
                        }
                    }
                    catch (NAudio.MmException ex)
                    {
                        if (ex.Result == NAudio.MmResult.AcmNotPossible)
                            _logger.LogError($"{oggFilePath.GAC_ToLocalPath()}\n" +
                                "OGG file conversion error! " +
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
            }

            await Task.Yield();

            if (File.Exists(newOggFilePath) && File.Exists(oggFilePath))
            {
                long oldFileSize = new FileInfo(oggFilePath).Length;
                long newFileSize = new FileInfo(newOggFilePath).Length;

                if (newFileSize < oldFileSize)
                {
                    File.Delete(oggFilePath);
                    File.Copy(newOggFilePath, oggFilePath);

                    _logger.LogInformation($"Successful file compression: {oggFilePath.GAC_ToLocalPath()}");
                }
                else
                    _logger.LogError($"OGG compression failed: {oggFilePath.GAC_ToLocalPath()}");

                File.Delete(newOggFilePath);
            }
        }
    }
}

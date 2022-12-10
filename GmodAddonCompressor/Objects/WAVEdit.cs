using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
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
    }
}

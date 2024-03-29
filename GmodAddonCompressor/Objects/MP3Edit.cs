﻿using GmodAddonCompressor.CustomExtensions;
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
    internal class MP3Edit : ICompress
    {
        private readonly ILogger _logger = LogSystem.CreateLogger<WAVEdit>();

        public async Task Compress(string mp3FilePath)
        {
            string newMp3FilePath = mp3FilePath + "____TEMP.mp3";

            if (File.Exists(newMp3FilePath))
                File.Delete(newMp3FilePath);

            if (AudioContext.UseFFMpegForCompress)
            {
                bool hasCompress = await new FFMpegSystem().CompressAudioAsync(mp3FilePath, newMp3FilePath, AudioContext.SamplingFrequency);
                if (hasCompress)
                {
                    _logger.LogInformation($"Successful file compression: {mp3FilePath.GAC_ToLocalPath()}");
                    return;
                }
            }

            using (var reader = new Mp3FileReader(mp3FilePath))
            {
                WaveFormat currentFormet = reader.WaveFormat;
                int rateNumber = AudioContext.SamplingFrequency;

                if (currentFormet.SampleRate <= rateNumber)
                    return;

                Mp3Frame frame = reader.ReadNextFrame();
                var newFormat = new Mp3WaveFormat(rateNumber, 1, frame.FrameLength, 16);

                try
                {
                    using (var c = new WaveFormatConversionStream(newFormat, reader))
                    {
                        WaveFileWriter.CreateWaveFile(newMp3FilePath, c);
                    }
                }
                catch (NAudio.MmException ex)
                {
                    if (ex.Result == NAudio.MmResult.AcmNotPossible)
                        _logger.LogError($"{mp3FilePath.GAC_ToLocalPath()}\n" +
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

                if (File.Exists(newMp3FilePath))
                {
                    long oldFileSize = new FileInfo(mp3FilePath).Length;
                    long newFileSize = new FileInfo(newMp3FilePath).Length;

                    if (newFileSize < oldFileSize)
                    {
                        File.Delete(mp3FilePath);
                        File.Copy(newMp3FilePath, mp3FilePath);
                        hasCompress = true;
                    }
                }

                if (!hasCompress && AudioContext.UseFFMpegForCompress)
                    hasCompress = await new FFMpegSystem().CompressAudioAsync(mp3FilePath, newMp3FilePath, AudioContext.SamplingFrequency);

                if (hasCompress)
                    _logger.LogInformation($"Successful file compression: {mp3FilePath.GAC_ToLocalPath()}");
                else
                    _logger.LogError($"MP3 compression failed: {mp3FilePath.GAC_ToLocalPath()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            if (File.Exists(newMp3FilePath))
                File.Delete(newMp3FilePath);
        }
    }
}

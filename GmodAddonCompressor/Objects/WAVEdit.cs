using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class WAVEdit
    {
        private int _rateNumber = 22050;

        internal int RateNumber
        {
            get { return _rateNumber; }
            set
            {
                _rateNumber = value < 16000 ? 16000 : value > 44100 ? 44100 : value;
            }
        }

        internal async Task WavCompress(string wavFilePath)
        {
            string newWavFilePath = wavFilePath + "_new.wav";

            await Task.Yield();

            // 44100 - Высокое качество
            // 32000 - Нормальное качество
            // 22050 - Среднее качество
            // 16000 - Низкое качество

            using (var reader = new WaveFileReader(wavFilePath))
            {
                var currentRate = reader.WaveFormat.SampleRate;

                if (currentRate > _rateNumber)
                {
                    var newFormat = new WaveFormat(_rateNumber, 16, 1);

                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            using (var s = new RawSourceWaveStream(stream, newFormat))
                            {
                                using (var c = new WaveFormatConversionStream(WaveFormat.CreateALawFormat(_rateNumber, 1), s))
                                {
                                    WaveFileWriter.CreateWaveFile(newWavFilePath, c);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            await Task.Yield();

            if (File.Exists(newWavFilePath) && File.Exists(wavFilePath))
            {
                long oldFileSize = new FileInfo(wavFilePath).Length;
                long newFileSize = new FileInfo(newWavFilePath).Length;

                await Task.Yield();

                if (newFileSize < oldFileSize)
                {
                    File.Delete(wavFilePath);
                    File.Copy(newWavFilePath, wavFilePath);
                }
                else
                    Console.WriteLine($"WAV compression failed: {wavFilePath}");

                File.Delete(newWavFilePath);
            }

            await Task.Yield();
        }
    }
}

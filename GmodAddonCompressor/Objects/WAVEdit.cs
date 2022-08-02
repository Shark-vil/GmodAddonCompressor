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
            string tempWavFilePath = wavFilePath + "_temp.wav";
            string newWavFilePath = wavFilePath + "_new.wav";

            if (!File.Exists(tempWavFilePath))
                File.Copy(wavFilePath, tempWavFilePath);

            await Task.Yield();

            // 44100 - Высокое качество
            // 32000 - Нормальное качество
            // 22050 - Среднее качество
            // 16000 - Низкое качество

            using (var reader = new WaveFileReader(wavFilePath))
            {
                var currentRate = reader.WaveFormat.SampleRate;

                Console.WriteLine($"Current rate: {currentRate}");

                if (currentRate > _rateNumber)
                {
                    //var newFormat = new WaveFormat(_rateNumber, 16, 1);

                    var newFormat = new WaveFormat(
                        _rateNumber,
                        reader.WaveFormat.BitsPerSample, // 16
                        reader.WaveFormat.Channels // 1
                    );

                    try
                    {
                        using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                        {
                            WaveFileWriter.CreateWaveFile(newWavFilePath, conversionStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            await Task.Yield();

            if (File.Exists(newWavFilePath))
            {
                if (File.Exists(wavFilePath))
                    File.Delete(wavFilePath);

                await Task.Yield();

                File.Copy(newWavFilePath, wavFilePath);
                File.Delete(newWavFilePath);
            }

            await Task.Yield();

            if (File.Exists(tempWavFilePath))
                File.Delete(tempWavFilePath);
        }
    }
}

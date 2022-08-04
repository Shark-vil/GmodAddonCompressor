using GmodAddonCompressor.Interfaces;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class MP3Edit : ICompress
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

        public async Task Compress(string mp3FilePath)
        {
            string tempMp3FilePath = mp3FilePath + "_temp.mp3";
            string newMp3FilePath = mp3FilePath + "_new.mp3";

            if (!File.Exists(tempMp3FilePath))
                File.Copy(mp3FilePath, tempMp3FilePath);

            await Task.Yield();

            using (var reader = new Mp3FileReader(mp3FilePath))
            {
                var currentRate = reader.WaveFormat.SampleRate;

                Console.WriteLine($"Current rate: {currentRate}");

                if (currentRate > _rateNumber)
                {
                    Mp3Frame frame = reader.ReadNextFrame();

                    //var newFormat = new Mp3WaveFormat(_rateNumber, 1, frame.FrameLength, 16);

                    var newFormat = new Mp3WaveFormat(
                        _rateNumber,
                        reader.WaveFormat.Channels, // 1
                        frame.FrameLength,
                        reader.WaveFormat.BitsPerSample // 16
                    );

                    try
                    {
                        using (var conversionStream = WaveFormatConversionStream.CreatePcmStream(reader))
                        {
                            WaveFileWriter.CreateWaveFile(newMp3FilePath, conversionStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            await Task.Yield();

            if (File.Exists(newMp3FilePath))
            {
                if (File.Exists(mp3FilePath))
                    File.Delete(mp3FilePath);

                await Task.Yield();

                File.Copy(newMp3FilePath, mp3FilePath);
                File.Delete(newMp3FilePath);
            }

            await Task.Yield();

            if (File.Exists(tempMp3FilePath))
                File.Delete(tempMp3FilePath);
        }
    }
}

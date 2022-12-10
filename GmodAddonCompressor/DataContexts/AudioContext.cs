namespace GmodAddonCompressor.DataContexts
{
    internal class AudioContext
    {
        private static int _samplingFrequency = 22050;

        internal static bool UseFFMpegForCompress;

        internal static int SamplingFrequency
        {
            get { return _samplingFrequency; }
            set
            {
                _samplingFrequency = value < 16000 ? 16000 : value > 44100 ? 44100 : value;
            }
        }
    }
}

namespace GmodAddonCompressor.DataContexts
{
    internal class AudioContext
    {
        private static int _rateNumber = 22050;

        internal static bool UseFFMpegForCompress;

        internal static int RateNumber
        {
            get { return _rateNumber; }
            set
            {
                _rateNumber = value < 16000 ? 16000 : value > 44100 ? 44100 : value;
            }
        }
    }
}

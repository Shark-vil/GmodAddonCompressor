namespace GmodAddonCompressor.DataContexts
{
    internal class ImageContext
    {
        private static int _resolution = 4;

        internal static int Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value < 2 ? 2 : value > 6 ? 6 : value;
            }
        }

        internal static uint MinimumSizeLimit = 256;
        internal static uint SkipWidth = 0;
        internal static uint SkipHeight = 0;
    }
}

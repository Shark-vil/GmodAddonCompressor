using GmodAddonCompressor.Bases;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class PNGEdit : ImageEditBase
    {
        public PNGEdit()
        {
            SetImageFileExtension(".png");
        }

        internal async Task PngCompress(string pngFilePath)
        {
            await ImageCompress(pngFilePath);
        }
    }
}

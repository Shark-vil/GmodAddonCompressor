using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class PNGEdit : ImageEditBase, ICompress
    {
        public PNGEdit()
        {
            SetImageFileExtension(".png");
        }

        public async Task Compress(string pngFilePath)
        {
            await ImageCompress(pngFilePath);
        }
    }
}

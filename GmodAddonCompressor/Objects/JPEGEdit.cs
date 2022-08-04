using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPEGEdit : ImageEditBase, ICompress
    {
        public JPEGEdit()
        {
            SetImageFileExtension(".jpeg");
        }

        public async Task Compress(string jpegFilePath)
        {
            await ImageCompress(jpegFilePath);
        }
    }
}

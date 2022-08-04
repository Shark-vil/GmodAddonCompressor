using GmodAddonCompressor.Bases;
using GmodAddonCompressor.Interfaces;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPGEdit : ImageEditBase, ICompress
    {
        public JPGEdit()
        {
            SetImageFileExtension(".jpg");
        }

        public async Task Compress(string jpgFilePath)
        {
            await ImageCompress(jpgFilePath);
        }
    }
}

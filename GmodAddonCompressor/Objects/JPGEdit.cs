using GmodAddonCompressor.Bases;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPGEdit : ImageEditBase
    {
        public JPGEdit()
        {
            SetImageFileExtension(".jpg");
        }

        internal async Task JpgCompress(string jpgFilePath)
        {
            await ImageCompress(jpgFilePath);
        }
    }
}

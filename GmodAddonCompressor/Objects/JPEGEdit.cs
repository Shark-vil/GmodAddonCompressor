using GmodAddonCompressor.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class JPEGEdit : ImageEditBase
    {
        public JPEGEdit()
        {
            SetImageFileExtension(".jpeg");
        }

        internal async Task JpegCompress(string jpegFilePath)
        {
            await ImageCompress(jpegFilePath);
        }
    }
}

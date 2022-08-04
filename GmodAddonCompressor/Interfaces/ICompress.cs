using System.Threading.Tasks;

namespace GmodAddonCompressor.Interfaces
{
    internal interface ICompress
    {
        Task Compress(string filePath);
    }
}

using GmodAddonCompressor.DataContexts;

namespace GmodAddonCompressor.CustomExtensions
{
    public static class StringExtension
    {
        public static string GAC_ToLocalPath(this string str)
        {
            return CompressDirectoryContext.ToLocal(str);
        }
    }
}

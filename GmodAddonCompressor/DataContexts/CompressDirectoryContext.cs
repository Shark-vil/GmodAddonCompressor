namespace GmodAddonCompressor.DataContexts
{
    internal class CompressDirectoryContext
    {
        private static string _directoryPath = string.Empty;

        internal static string DirectoryPath
        {
            set
            {
                _directoryPath = value;

                if (!_directoryPath.EndsWith("\\"))
                    _directoryPath += "\\";
            }
            get { return _directoryPath; }
        }

        internal static string ToLocal(string fullPath) => fullPath.Replace(_directoryPath, string.Empty);
    }
}

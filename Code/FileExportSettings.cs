namespace Enigmatic.Core
{
    public struct FileExportSettings
    {
        public string FileName;
        public string Path;

        public FileExportSettings(string name, string path)
        {
            FileName = name;
            Path = path;
        }
    }
}

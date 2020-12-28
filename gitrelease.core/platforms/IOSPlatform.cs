using Claunia.PropertyList;
using System.IO;
using System.Linq;

namespace gitrelease.platforms
{
    internal class IOSPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.IOS;

        public IOSPlatform(string path)
        {
            this.path = path;
        }

        public ReleaseSequenceFlags Release(string version)
        {
            if(!Directory.Exists(this.path))
            {
                return ReleaseSequenceFlags.InvalidDirectory;
            }

            var plistFilePath = GetPlistFilePath(this.path);

            if (!File.Exists(plistFilePath))
            {
                return ReleaseSequenceFlags.FileNotFound;
            }

            NSDictionary rootDict = (NSDictionary)PropertyListParser.Parse(plistFilePath);

            (rootDict["CFBundleShortVersionString"] as NSString).Content = version;

            PropertyListParser.SaveAsXml(rootDict, new FileInfo(plistFilePath));

            return ReleaseSequenceFlags.Ok;
        }

        private string GetPlistFilePath(string path)
        {
            return Directory.GetFiles(path, "Info.plist").FirstOrDefault();
        }
    }
}
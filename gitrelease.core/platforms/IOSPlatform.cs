using Claunia.PropertyList;
using System.IO;
using System.Linq;

namespace gitrelease.core.platforms
{
    internal class IOSPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.IOS;

        public IOSPlatform(string path)
        {
            this.path = path;
        }

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(string version)
        {
            if(!Directory.Exists(this.path))
            {
                return (ReleaseManagerFlags.InvalidDirectory, new string[] { });
            }

            var plistFilePath = GetPlistFilePath(this.path);

            if (!File.Exists(plistFilePath))
            {
                return (ReleaseManagerFlags.FileNotFound, new string[] { });
            }

            var rootDict = (NSDictionary)PropertyListParser.Parse(plistFilePath);

            if (rootDict?["CFBundleShortVersionString"] is NSString nsString)
            {
                nsString.Content = version;
            }

            PropertyListParser.SaveAsXml(rootDict, new FileInfo(plistFilePath));

            return (ReleaseManagerFlags.Ok, new string[]{plistFilePath});
        }

        public string GetVersion()
        {
            if (!Directory.Exists(this.path))
            {
                return "Invalid directory";
            }

            var plistFilePath = GetPlistFilePath(this.path);

            if (!File.Exists(plistFilePath))
            {
                return "Invalid directory";
            }

            var rootDict = (NSDictionary)PropertyListParser.Parse(plistFilePath);

            if (rootDict?["CFBundleShortVersionString"] is NSString nsString)
            {
                return nsString.Content;
            }

            return "Invalid directory";
        }

        private string GetPlistFilePath(string path)
        {
            return Directory.GetFiles(path, "Info.plist").FirstOrDefault();
        }
    }
}
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

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(GitVersion version)
        {
            if(!Directory.Exists(path))
            {
                return (ReleaseManagerFlags.InvalidDirectory, new string[] { });
            }

            var plistFilePath = GetPlistFilePath(path);

            if (!File.Exists(plistFilePath))
            {
                return (ReleaseManagerFlags.FileNotFound, new string[] { });
            }

            var rootDict = (NSDictionary)PropertyListParser.Parse(plistFilePath);

            if(rootDict == null)
                return (ReleaseManagerFlags.FileNotFound, new string[] { });

            if (!rootDict.ContainsKey("CFBundleShortVersionString"))
            {
                rootDict.Add("CFBundleShortVersionString", version);
            }
            else if (rootDict["CFBundleShortVersionString"] is NSString nsString)
            {
                nsString.Content = version.ToMajorMinorPatch();
            }

            if (!rootDict.ContainsKey("CFBundleVersion"))
            {
                rootDict.Add("CFBundleVersion", version);
            }
            else if (rootDict["CFBundleVersion"] is NSString nsString)
            {
                nsString.Content = version.ToMajorMinorPatch();
            }

            PropertyListParser.SaveAsXml(rootDict, new FileInfo(plistFilePath));

            return (ReleaseManagerFlags.Ok, new string[]{plistFilePath});
        }

        public string GetVersion()
        {
            if (!Directory.Exists(path))
            {
                return "Invalid directory";
            }

            var plistFilePath = GetPlistFilePath(path);

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
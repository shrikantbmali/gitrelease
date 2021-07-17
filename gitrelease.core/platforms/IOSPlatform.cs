using Claunia.PropertyList;
using System.IO;
using System.Linq;

namespace gitrelease.core.platforms
{
    internal class IOSPlatform : IPlatform
    {
        private const string PlistFile = "Info.plist";

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
                rootDict.Add("CFBundleShortVersionString", version.ToVersionString());
            }
            else if (rootDict["CFBundleShortVersionString"] is NSString nsString)
            {
                nsString.Content = version.GetVersionWithoutBuildNumber().ToVersionString();
            }

            if (!rootDict.ContainsKey("CFBundleVersion"))
            {
                rootDict.Add("CFBundleVersion", version.ToVersionString());
            }
            else if (rootDict["CFBundleVersion"] is NSString nsString)
            {
                nsString.Content = version.BuildNumber;
            }

            PropertyListParser.SaveAsXml(rootDict, new FileInfo(plistFilePath));

            return (ReleaseManagerFlags.Ok, new[]{plistFilePath});
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

            if (rootDict?["CFBundleVersion"] is NSString nsString)
            {
                return $"{Type}:{nsString.Content}";
            }

            return "Invalid directory";
        }

        private static string GetPlistFilePath(string path)
        {
            return Directory.GetFiles(path, PlistFile).FirstOrDefault();
        }

        public static bool IsValid(string path, string root)
        {
            return File.Exists(Path.Combine(Path.IsPathRooted(path) ? path : Path.Combine(root, path), PlistFile));
        }
    }
}
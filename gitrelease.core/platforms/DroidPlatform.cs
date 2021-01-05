using System.IO;
using System.Linq;
using System.Xml;

namespace gitrelease.core.platforms
{
    internal class DroidPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.Droid;

        public DroidPlatform(string path)
        {
            this.path = path;
        }

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(GitVersion version)
        {
            if (!Directory.Exists(path))
            {
                return (ReleaseManagerFlags.InvalidDirectory, new string[] { });
            }

            var manifestFilePath = GetManifestFilePath(path);

            if (!File.Exists(manifestFilePath))
            {
                return (ReleaseManagerFlags.FileNotFound, new string[] { });
            }

            var xml = LoadManifest(manifestFilePath);

            if (xml != null)
            {
                var versionName = xml?.SelectNodes("/manifest")?.Item(0)?.Attributes?["android:versionName"];
                var versionCode = xml?.SelectNodes("/manifest")?.Item(0)?.Attributes?["android:versionCode"];

                if (versionName == null || versionCode == null)
                    return (ReleaseManagerFlags.InvalidAndroidManifestFileMissingAttributes, new string[] { });

                versionName.InnerText = version.ToVersionString();
                versionCode.InnerText = version.BuildNumber;

                xml?.Save(manifestFilePath);

                return (ReleaseManagerFlags.Ok, new[] {manifestFilePath});
            }

            return (ReleaseManagerFlags.Unknown, new string[] { });
        }

        private static XmlDocument LoadManifest(string manifestFilePath)
        {
            var xml = new XmlDocument();
            xml.Load(manifestFilePath);
            return xml;
        }

        public string GetVersion()
        {
            if (!Directory.Exists(path))
            {
                return $"{Type}: manifest not found";
            }

            var manifestFilePath = GetManifestFilePath(path);

            if (!File.Exists(manifestFilePath))
            {
                return $"{Type}: manifest not found";
            }

            var xml = LoadManifest(manifestFilePath);
            var node = xml?.SelectNodes("/manifest")?.Item(0)?.Attributes?["android:versionName"];

            return $"{Type}:{node?.InnerText}";
        }

        private static string GetManifestFilePath(string path)
        {
            return Directory.GetFiles(path, @"Properties\AndroidManifest.xml").FirstOrDefault();
        }
    }
}
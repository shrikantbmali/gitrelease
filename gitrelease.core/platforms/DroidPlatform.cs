using System.IO;
using System.Linq;
using System.Xml;

namespace gitrelease.platforms
{
    internal class DroidPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.Droid;

        public DroidPlatform(string path)
        {
            this.path = path;
        }

        public ReleaseSequenceFlags Release()
        {
            if (!Directory.Exists(this.path))
            {
                return ReleaseSequenceFlags.InvalidDirectory;
            }

            var manifestFilePath = GetManifestFilePath(this.path);

            if (!File.Exists(manifestFilePath))
            {
                return ReleaseSequenceFlags.FileNotFound;
            }

            var xml = new XmlDocument();
            xml.Load(manifestFilePath);

            XmlAttribute node = xml.SelectNodes("/manifest").Item(0).Attributes["android:versionName"];

            node.InnerText = "I am inner text";

            xml.Save(manifestFilePath);
            return ReleaseSequenceFlags.Ok;
        }

        private string GetManifestFilePath(string path)
        {
            return Directory.GetFiles(path, @"Properties\AndroidManifest.xml").FirstOrDefault();
        }
    }
}
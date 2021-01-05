#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;

namespace gitrelease.core.platforms
{
    internal class UWPPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.UWP;
        
        public UWPPlatform(string path)
        {
            this.path = path;
        }

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(GitVersion version)
        {
            try
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

                var versionNode = RetrieveVersionNode(xml);

                if (versionNode != null)
                    versionNode.InnerText = version.ToUWPSpecificVersionString();
                else
                    return (ReleaseManagerFlags.InvalidUWPPackageFile, new string[] { });

                xml?.Save(manifestFilePath);

                return (ReleaseManagerFlags.Ok, new[] {manifestFilePath});

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (ReleaseManagerFlags.Unknown, new string[] { });
            }
        }

        private static XmlAttribute? RetrieveVersionNode(XmlNode xml)
        {
            var node = xml?.SelectNodes("/");

            var xmlNodes = node?.Item(0);

            if (xmlNodes == null)
                return null;
            
            foreach (var xmlElement in xmlNodes.Cast<XmlNode>().Where(xmlElement => xmlElement.Name?.ToLower() == "package"))
            {
                foreach (XmlNode childNode in xmlElement.ChildNodes)
                {
                    if (childNode?.Name?.ToLower() == "identity")
                    {
                        return childNode?.Attributes?["Version"];
                    }
                }

                break;
            }

            return null;
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

            return $"{Type}:{RetrieveVersionNode(xml)?.InnerText}";
        }

        private static string GetManifestFilePath(string path)
        {
            return Directory.GetFiles(path, "Package.appxmanifest").FirstOrDefault();
        }
    }

    internal static class GitVersionUwpExtension
    {
        public static string ToUWPSpecificVersionString(this GitVersion version)
        {
            return $"{version.Major}.{version.Minor}.{version.Patch}"
                   + "." + (!string.IsNullOrEmpty(version.BuildNumber) ? version.BuildNumber : "0");
        }

        private static object GetPreReleaseSpecificNumber(string versionPreReleaseTag)
        {
            return versionPreReleaseTag switch
            {
                "alpha" => 1,
                "beta" => 2,
                "rc" => 3,
                _ => int.MaxValue
            };
        }
    }
}
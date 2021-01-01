using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gitrelease.core
{
    internal class Package
    {
        private readonly string _rootDir;

        public Package(string rootDir)
        {
            _rootDir = rootDir;
        }

        public GitVersion GetVersion() => GetPackageVersion();

        private GitVersion GetPackageVersion()
        {
            var json = ReadFile(GetPackageFilePath());

            return new GitVersion(json["version"]?.Value<string>() ?? string.Empty);
        }

        public ReleaseManagerFlags SetVersion(GitVersion version)
        {
            var result = UpdatePackageFile(version);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            return UpdateVersionFile(version.ToMajorMinorPatch());
        }

        private ReleaseManagerFlags UpdatePackageFile(GitVersion version)
        {
            var packageFilePath = GetPackageFilePath();

            var json = ReadFile(packageFilePath);
            json["version"] = version.ToMajorMinorPatch();

            return SaveFile(json, packageFilePath);
        }

        private ReleaseManagerFlags UpdateVersionFile(string versionString)
        {
            var versionFile = GetVersionFile();

            var json = ReadFile(versionFile);
            json["version"] = versionString;
            
            return SaveFile(json, versionFile);
        }

        private static JObject ReadFile(string path) => JObject.Parse(File.ReadAllText(path));

        private static ReleaseManagerFlags SaveFile(JObject json, string filePath)
        {
            try
            {
                var js = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(filePath, js);
                return ReleaseManagerFlags.Ok;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                return ReleaseManagerFlags.PackageJsonVersionUpdateFailed;
            }
        }

        public ConfigFile GetConfig()
        {
            return Config.ParseFile(Path.Combine(_rootDir, ConfigFile.FixName));
        }

        public bool AreToolsAvailable()
        {
            var result = CommandExecutor.ExecuteCommand("npm", "", _rootDir);

            return result.isError;
        }

        public bool IsInitialized()
        {
            var packageFile = GetPackageFilePath();
            var versionFile = GetVersionFile();

            return File.Exists(packageFile) && File.Exists(versionFile);
        }

        private string GetVersionFile()
        {
            return Path.Combine(_rootDir, "version.json");
        }

        private string GetPackageFilePath()
        {
            return Path.Combine(_rootDir, "package.json");
        }
    }
}
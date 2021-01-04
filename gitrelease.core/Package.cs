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

        public ReleaseManagerFlags SetVersion(GitVersion version, bool isNativeProject)
        {
            var result = UpdatePackageFile(version);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            if (!isNativeProject)
                result = UpdateVersionFile(version.ToVersionString());
            
            return result;
        }

        private ReleaseManagerFlags UpdatePackageFile(GitVersion version)
        {
            var packageFilePath = GetPackageFilePath();

            var json = ReadFile(packageFilePath);
            json["version"] = version.ToVersionString();

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
            return Config.ParseFile(GetConfigFilePath());
        }

        private string GetConfigFilePath()
        {
            return Path.Combine(_rootDir, ConfigFile.FixName);
        }

        public bool AreToolsAvailable()
        {
            var result = CommandExecutor.ExecuteCommand("npm", "", _rootDir);

            return result.isError;
        }

        public bool IsInitialized()
        {
            if (!File.Exists(GetConfigFilePath()))
                return false;

            var configFile = GetConfig();

            if (!configFile.IsGenericProject && !File.Exists(GetVersionFile()))
                return false;

            return File.Exists(GetPackageFilePath());
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
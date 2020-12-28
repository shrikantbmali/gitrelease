using gitrelease.platforms;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace gitrelease
{
    internal class ReleaseManager : IReleaseManager
    {
        private IEnumerable<IPlatform> platforms;
        private string rootDirectory;

        public ReleaseManager(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public ReleaseManagerFlags Initialize()
        {
            if (!Directory.Exists(this.rootDirectory))
            {
                return ReleaseManagerFlags.InvalidRootDir;
            }

            if(!ConfigFileExists(this.rootDirectory, out string configFilePath))
            {
                return ReleaseManagerFlags.ConfigurationNotFound;
            }

            if (!TryParseFile(configFilePath, out var config))
            {
                return ReleaseManagerFlags.InvalidFile;
            }

            this.platforms = CreatePlatforms(config.Platforms).ToArray();

            if (this.platforms.Any(platform => platform.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidPlatform;
            }

            return ReleaseManagerFlags.Ok;
        }

        public ReleaseSequenceFlags Release()
        {
            return Transaction(repo => GitSanity(
                repo, repo => GitReleaser.PrepareRelease(
                    repo, version => StartReleasing(version))), ReleaseSequenceFlags.Unknown);
        }

        private ReleaseSequenceFlags StartReleasing(string version)
        {
            foreach (var platform in this.platforms)
            {
                ReleaseSequenceFlags flag = platform.Release(version.ToString());

                if (flag != ReleaseSequenceFlags.Ok)
                {
                    return flag;
                }
            }

            return ReleaseSequenceFlags.Ok;
        }

        private static bool ConfigFileExists(string rootPath, out string filePath)
        {
            filePath = Path.Combine(rootPath, ConfigFile.FixName);
            return File.Exists(filePath);
        }

        private static ReleaseSequenceFlags GitSanity(Repository repo, Func<Repository, ReleaseSequenceFlags> func)
        {
            try
            {
                var status = repo.RetrieveStatus(new StatusOptions());

                if (!status.IsDirty)
                {
                    return func(repo);
                }

                return ReleaseSequenceFlags.DirtyRepo;
            }
            catch (Exception ex)
            {
            }

            return ReleaseSequenceFlags.Unknown;
        }

        private ReleaseSequenceFlags Transaction(Func<Repository, ReleaseSequenceFlags> func, ReleaseSequenceFlags failFlag)
        {
            Repository repo = null;

            try
            {
                repo = new Repository(this.rootDirectory);

                return func(repo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return failFlag;
            }
            finally
            {
                repo?.Dispose();
            }
        }

        private IEnumerable<IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            foreach (var platform in platforms)
            {
                var absoutePath = Path.Combine(this.rootDirectory, platform.Path);
                switch (platform.Name?.ToLower())
                {
                    case "dll":
                        yield return new DLLPlatform(absoutePath);
                        break;
                    case "ios":
                        yield return new IOSPlatform(absoutePath);
                        break;
                    case "droid":
                        yield return new DroidPlatform(absoutePath);
                        break;
                    case "uwp":
                        yield return new UWPPlatform(absoutePath);
                        break;
                    default:
                        yield return new Invalid(absoutePath);
                        break;
                }
            }
        }

        private static bool TryParseFile(string filePath, out ConfigFile file)
        {
            file = default;
            try
            {
                file = JsonSerializer.Deserialize<ConfigFile>(File.ReadAllText(filePath));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
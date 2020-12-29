using gitrelease.core.platforms;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace gitrelease.core
{
    internal class ReleaseManager : IReleaseManager
    {
        private ReadOnlyDictionary<string, IPlatform> platforms;
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

            this.platforms = CreatePlatforms(config.Platforms);

            if (this.platforms.Any(platform => platform.Value.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidPlatform;
            }

            return ReleaseManagerFlags.Ok;
        }

        public ReleaseManagerFlags Release()
        {
            return Transaction(repo => GitSanity(
                repo, repo => GitReleaser.PrepareRelease(
                    repo, StartReleasing)), ReleaseManagerFlags.Unknown);
        }

        public string[] GetVersion(string platformName)
        {
            return platformName == "all"
                ? this.platforms.Select(p => p.Value.GetVersion()).ToArray()
                : new[] {this.platforms[platformName].GetVersion()};
        }

        private ReleaseManagerFlags StartReleasing(string version)
        {
            foreach (var platform in this.platforms)
            {
                ReleaseManagerFlags flag = platform.Value.Release(version);

                if (flag != ReleaseManagerFlags.Ok)
                {
                    return flag;
                }
            }

            return ReleaseManagerFlags.Ok;
        }

        private static bool ConfigFileExists(string rootPath, out string filePath)
        {
            filePath = Path.Combine(rootPath, ConfigFile.FixName);
            return File.Exists(filePath);
        }

        private static ReleaseManagerFlags GitSanity(Repository repo, Func<Repository, ReleaseManagerFlags> func)
        {
            try
            {
                var status = repo.RetrieveStatus(new StatusOptions());

                if (!status.IsDirty)
                {
                    var releasingFlags = func(repo);

                    if(releasingFlags == ReleaseManagerFlags.Ok)
                    {
                        return CreateReleaseCommit(repo);
                    }

                    return releasingFlags;
                }

                return ReleaseManagerFlags.DirtyRepo;
            }
            catch (Exception ex)
            {
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags CreateReleaseCommit(Repository repo)
        {
            var head = repo.Head;
            ReleaseManagerFlags result = ReleaseManagerFlags.Unknown;

            try
            {
                result = Stage(repo);
                if (result == ReleaseManagerFlags.Ok)
                {
                    result = Commit(repo);
                }
            }
            catch (Exception)
            {
                result = ReleaseManagerFlags.Unknown;
            }

            if(result != ReleaseManagerFlags.Ok)
            {
                repo.Reset(ResetMode.Hard, head.Tip);
            }

            return result;
        }

        private static ReleaseManagerFlags Commit(Repository repo)
        {
            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit("chore(AppVersion): App version updated.", sign, sign);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception)
            {
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags Stage(Repository repo)
        {
            try
            {
                var status = repo.RetrieveStatus();

                foreach (var file in status.Modified.Select(mods => mods.FilePath))
                {
                    repo.Index.Add(file);
                }

                repo.Index.Write();

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception)
            {
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags Transaction(Func<Repository, ReleaseManagerFlags> func, ReleaseManagerFlags failFlag)
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

        private ReadOnlyDictionary<string, IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            var ps = new Dictionary<string, IPlatform>();

            foreach (var platform in platforms)
            {
                var absolutePath = Path.Combine(this.rootDirectory, platform.Path);

                switch (platform.Name?.ToLower())
                {
                    case "dll":
                        ps.Add(platform.Name, new DLLPlatform(absolutePath));
                        break;
                    case "ios":
                        ps.Add(platform.Name, new IOSPlatform(absolutePath));
                        break;
                    case "droid":
                        ps.Add(platform.Name, new DroidPlatform(absolutePath));
                        break;
                    case "uwp":
                        ps.Add(platform.Name, new UWPPlatform(absolutePath));
                        break;
                    default:
                        ps.Add("null", new Invalid(absolutePath));
                        break;
                }
            }

            return new ReadOnlyDictionary<string, IPlatform>(ps);
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
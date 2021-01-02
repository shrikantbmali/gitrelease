using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using gitrelease.core.platforms;
using LibGit2Sharp;

namespace gitrelease.core
{
    public class ReleaseManager
    {
        private readonly string _rootDir;
        private Package _package;

        public ReleaseManager(string rootDir)
        {
            _rootDir = rootDir;
        }

        public ReleaseManagerFlags Initialize()
        {
            if (!Directory.Exists(_rootDir))
            {
                return ReleaseManagerFlags.InvalidDirectory;
            }

            if (_package != null)
            {
                return ReleaseManagerFlags.AlreadyInitialized;
            }

            _package = new Package(_rootDir);

            return ReleaseManagerFlags.Ok;
        }

        public ReleaseManagerFlags Release(ReleaseChoices releaseChoices)
        {
            return ExecuteSafe(() =>
            {
                return ExecuteRepoSafe(repo =>
                {
                    Console.WriteLine("Validating repo...");
                    var validityFlag = IsRepoReadyForRelease(repo);

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    var config = _package.GetConfig();

                    var result = config == null
                        ? ReleaseManagerFlags.InvalidConfigFile
                        : Release(repo, config, releaseChoices);

                    if(result == ReleaseManagerFlags.Ok)
                        Console.WriteLine("To push changes to origin, use command: git push && git push --tags");

                    return result;
                });
            });
        }

        private ReleaseManagerFlags Release(IRepository repo, ConfigFile configFile, ReleaseChoices releaseChoices)
        {
            Console.WriteLine("Starting release sequence.");

            var version = DetermineNextVersion(repo, releaseChoices);

            Console.WriteLine("Updating package version...");
            var releaseFlag = UpdatePackageVersion(version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            Console.WriteLine("Updating platform version...");
            releaseFlag = UpdatePlatformVersions(version, configFile);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = CreateACommit(repo, version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            if (!releaseChoices.CustomVersion?.IsPrerelease() ?? false)
            {
                Console.WriteLine("Generating changelog...");
                releaseFlag = UpdateChangelog();
            }
            else
            {
                Console.WriteLine("Creation of changelog skipped due to it being a pre-release.");
            }

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            Console.WriteLine("Creating commit...");
            releaseFlag = AmendLastCommit(repo, version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            if (!releaseChoices.CustomVersion?.IsPrerelease() ?? false)
            {
                Console.WriteLine($"Creating tag name v{version.ToVersionString()}");
                releaseFlag = CreateTag(repo, version);
            }
            else
            {
                Console.WriteLine("Creation of tag skipped due to this being a pre-release.");
            }

            return releaseFlag;
        }

        private GitVersion DetermineNextVersion(IRepository repo, ReleaseChoices releaseChoices)
        {
            var version = GetCurrentVersion();

            var versionVNext = releaseChoices.ReleaseType == ReleaseType.Custom
                ? releaseChoices.CustomVersion
                : InfuseCommitAndIncrement(repo, version, releaseChoices.ReleaseType);

            if (releaseChoices.CustomVersion?.IsPrerelease() ?? false)
            {
                versionVNext = versionVNext.GetNewWithPreReleaseTag(releaseChoices.CustomVersion.PreReleaseTag);
            }

            Console.WriteLine($"Current version is: {version} and it will be updated to {versionVNext}");

            return versionVNext;
        }

        private static GitVersion InfuseCommitAndIncrement(IRepository repo, GitVersion gitVersion, ReleaseType releaseType)
        {
            //var headTipSha = repo.Head.Tip.Id.Sha.Substring(0, 10);
            return GetUpdateVersion(gitVersion, releaseType);
            //return new GitVersion(GetUpdateVersion(gitVersion, releaseType), headTipSha);
        }

        private static GitVersion GetUpdateVersion(GitVersion gitVersion, ReleaseType releaseType)
        {
            switch (releaseType)
            {
                case ReleaseType.Major:
                    return gitVersion.IncrementMajorAndGetNew();
                case ReleaseType.Minor:
                    return gitVersion.IncrementMinorAndGetNew();
                case ReleaseType.Patch:
                    return gitVersion.IncrementPatchAndGetNew();
                default:
                    throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null);
            }
        }

        private static ReleaseManagerFlags CreateACommit(IRepository repo, GitVersion version)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit($"chore(VersionUpdate): {version.ToVersionString()}", sign, sign);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags AmendLastCommit(IRepository repo, GitVersion version)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit($"chore(VersionUpdate): {version.ToVersionString()}", sign, sign, new CommitOptions()
                {
                    AmendPreviousCommit = true,
                });

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags Stage(IRepository repo)
        {
            try
            {
                var status = repo.RetrieveStatus();

                foreach (var file in
                    status.Modified.Select(entry => entry.FilePath)
                        .Union(status.Added.Select(entry => entry.FilePath)
                            .Union(status.Untracked.Select(entry => entry.FilePath))))
                {
                    repo.Index.Add(file);
                }

                repo.Index.Write();

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags CreateTag(IRepository repo, GitVersion version)
        {
            try
            {
                repo.ApplyTag($"v{version.ToVersionString()}");
                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ReleaseManagerFlags.TagCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdateChangelog()
        {
            try
            {
                var (_, isError) = CommandExecutor.ExecuteCommand("changelog", "generate", _rootDir);

                return isError ? ReleaseManagerFlags.ChangelogCreationFailed : ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ReleaseManagerFlags.ChangelogCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdatePlatformVersions(GitVersion version, ConfigFile configFile)
        {
            if (configFile?.Platforms == null)
                return ReleaseManagerFlags.InvalidConfigFile;

            var platforms = CreatePlatforms(configFile.Platforms);

            if (platforms.Any(platform => platform.Value.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidConfigFile;
            }

            foreach (var platform in platforms)
            {
                var (flag, _) = platform.Value.Release(version);

                if (flag != ReleaseManagerFlags.Ok)
                    return flag;
            }

            return ReleaseManagerFlags.Ok;
        }

        private ReadOnlyDictionary<string, IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            var ps = new Dictionary<string, IPlatform>();

            foreach (var platform in platforms)
            {
                var absolutePath = Path.Combine(_rootDir, platform.Path);

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

        private ReleaseManagerFlags UpdatePackageVersion(GitVersion version)
        {
            return _package.SetVersion(version);
        }

        private GitVersion GetCurrentVersion()
        {
            return _package.GetVersion();
        }

        private ReleaseManagerFlags IsRepoReadyForRelease(IRepository repo)
        {
            if (IsRepoDirty(repo))
            {
                return ReleaseManagerFlags.DirtyRepo;
            }

            if (!IsRepoInitialized())
            {
                return ReleaseManagerFlags.RepoNotInitializedForReleaseProcess;
            }

            if (AreToolsAvailable())
            {
                return ReleaseManagerFlags.InstallNpm;
            }

            return ReleaseManagerFlags.Ok;
        }

        private ReleaseManagerFlags IsRepoReadyForSetup(IRepository repo)
        {
            if (IsRepoDirty(repo))
            {
                return ReleaseManagerFlags.DirtyRepo;
            }

            if (IsRepoInitialized())
            {
                return ReleaseManagerFlags.RepoAlreadyInitialized;
            }

            if (AreToolsAvailable())
            {
                return ReleaseManagerFlags.InstallNpm;
            }

            return ReleaseManagerFlags.Ok;
        }

        private bool AreToolsAvailable()
        {
            return _package.AreToolsAvailable();
        }

        private bool IsRepoInitialized()
        {
            return _package.IsInitialized();
        }

        private static bool IsRepoDirty(IRepository repo)
        {
            return repo.RetrieveStatus(new StatusOptions())?.IsDirty ?? true;
        }

        private ReleaseManagerFlags ExecuteRepoSafe(Func<IRepository, ReleaseManagerFlags> func)
        {
            using var repo = new Repository(_rootDir);
            var repoHead = repo.Head;

            var result = ReleaseManagerFlags.Unknown;

            try
            {
                result = func(repo);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = ReleaseManagerFlags.Unknown;
            }
            finally
            {
                if (result != ReleaseManagerFlags.Ok && result != ReleaseManagerFlags.DirtyRepo)
                {
                    repo.Reset(ResetMode.Hard, repoHead.Tip);
                    repo.RemoveUntrackedFiles();
                }
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags ExecuteSafe(Func<ReleaseManagerFlags> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        public IEnumerable<string> GetVersion(string platformName)
        {
            return platformName == "all"
                ? CreatePlatforms(_package.GetConfig().Platforms).Select(p => p.Value.GetVersion()).ToArray()
                : new[] { CreatePlatforms(_package.GetConfig().Platforms)[platformName].GetVersion() };
        }

        public ReleaseManagerFlags SetupRepo()
        {
            return ExecuteSafe(() =>
            {
                return ExecuteRepoSafe(repo =>
                {
                    var validityFlag = IsRepoReadyForSetup(repo);

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InitNpm();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InstallChangelogGenerator();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InitNbgv();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InitDefaultConfig();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = Stage(repo);

                    return validityFlag;
                });
            });
        }

        private ReleaseManagerFlags InstallChangelogGenerator()
        {
            var (_, isError) =
                CommandExecutor.ExecuteCommand("npm", "install generate-changelog -D", _rootDir);

            if (isError)
                return ReleaseManagerFlags.ChangelogGeneratorInstallFailed;

            (_, isError) =
                CommandExecutor.ExecuteCommand("npm", "install", _rootDir);

            return isError ? ReleaseManagerFlags.ChangelogGeneratorInstallFailed : ReleaseManagerFlags.Ok;
        }

        private ReleaseManagerFlags InitDefaultConfig()
        {
            var save = new ConfigFile()
            {
                Platforms = new[]
                {
                    new Platform()
                    {
                        Name = "platform name, supported are [ios, droid, uwp]",
                        Path = "path to the root of the specified platforms project."
                    }
                }
            }.Save(Path.Combine(_rootDir, ConfigFile.FixName));

            return save ? ReleaseManagerFlags.Ok : ReleaseManagerFlags.ConfigFileCreationFailed;
        }

        private ReleaseManagerFlags InitNbgv()
        {
            var (_, isError) =
                CommandExecutor.ExecuteCommand("dotnet", "tool install --global nbgv --version 3.3.37", _rootDir);

            if (isError)
                return ReleaseManagerFlags.NBGVInstallationFailed;

            (_, isError) = CommandExecutor.ExecuteCommand("nbgv", "install", _rootDir);

            return isError ? ReleaseManagerFlags.NBGVInitFailed : ReleaseManagerFlags.Ok;
        }

        private ReleaseManagerFlags InitNpm()
        {
            var (_, isError) = CommandExecutor.ExecuteCommand("npm", "init -f", _rootDir);

            return isError ? ReleaseManagerFlags.NPMInitFailed : ReleaseManagerFlags.Ok;
        }
    }
}
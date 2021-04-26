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
        private readonly IMessenger _messenger;
        private Package _package;

        public ReleaseManager(string rootDir, IMessenger messenger)
        {
            _rootDir = rootDir;
            _messenger = messenger;
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
                return ExecuteRepoSafe(releaseChoices.IgnoreDirty, repo =>
                {
                    _messenger.Info("Validating repo...");
                    var validityFlag = IsRepoReadyForRelease(repo, releaseChoices);

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    var config = _package.GetConfig();

                    var result = config == null
                        ? ReleaseManagerFlags.InvalidConfigFile
                        : Release(repo, config, releaseChoices);

                    if(result == ReleaseManagerFlags.Ok && !releaseChoices.CustomVersion.IsPreRelease() && !releaseChoices.DryRun)
                        _messenger.Info("To push changes to origin, use command: git push && git push --tags");

                    return result;
                });
            });
        }

        private ReleaseManagerFlags Release(IRepository repo, ConfigFile configFile, ReleaseChoices releaseChoices)
        {
            _messenger.Info("Starting release sequence.");

            var packageVersion = _package.GetVersion();

            var nextVersion = DetermineNextVersion(repo, releaseChoices);

            _messenger.Info("Updating package version...");
            var releaseFlag = UpdatePackageVersion(nextVersion, configFile.IsGenericProject);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            _messenger.Info("Updating platform version...");
            releaseFlag = UpdatePlatformVersions(nextVersion, configFile);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            if (!releaseChoices.DryRun)
                releaseFlag = CreateACommit(repo, nextVersion, configFile);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            if (!releaseChoices.DryRun && !releaseChoices.SkipTag)
            {
                _messenger.Info($"Creating tag name v{nextVersion.ToVersionString()}");
                releaseFlag = CreateTag(repo, nextVersion);
            }

            if (!releaseChoices.SkipChangelog)
            {
                _messenger.Info("Generating changelog...");
                releaseFlag = UpdateChangelog(releaseChoices, configFile, packageVersion, nextVersion);
            }

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            if (!releaseChoices.DryRun)
            {
                _messenger.Info("Creating commit...");
                releaseFlag = AmendLastCommit(repo, nextVersion, configFile);
            }

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            return releaseFlag;
        }

        private GitVersion DetermineNextVersion(IRepository repo, ReleaseChoices releaseChoices)
        {
            var version = GetCurrentVersion(releaseChoices);

            var versionVNext = Increment(repo, version, releaseChoices.ReleaseType);

            if (releaseChoices.CustomVersion?.IsPreRelease() ?? false)
            {
                versionVNext = versionVNext.GetNewWithPreReleaseTag(releaseChoices.CustomVersion.PreReleaseTag);
            }

            _messenger.Info($"Current version is: {version} and it will be updated to {versionVNext}");

            return versionVNext;
        }

        private static GitVersion Increment(IRepository repo, GitVersion gitVersion, ReleaseType releaseType)
        {
            return GetUpdateVersion(gitVersion, releaseType, repo).SetBuildNumberAndGetNew(GetBuildNumber(repo));
        }

        private static GitVersion GetUpdateVersion(GitVersion gitVersion, ReleaseType releaseType,
            IRepository repo)
        {
            return releaseType switch
            {
                ReleaseType.Major => gitVersion.IncrementMajor().ResetMinor().ResetPatch(),
                ReleaseType.Minor => gitVersion.IncrementMinor().ResetPatch(),
                ReleaseType.Patch => gitVersion.IncrementPatch(),
                ReleaseType.Custom => gitVersion,
                ReleaseType.BuildNumber => gitVersion.SetBuildNumberAndGetNew(GetBuildNumber(repo)),
                _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
            };
        }

        private static int GetBuildNumber(IRepository repo)
        {
            return repo.Commits.Count();
        }

        private ReleaseManagerFlags CreateACommit(IRepository repo, GitVersion version, ConfigFile configFile)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = BuildSignature(repo, configFile);
                repo.Commit($"chore(VersionUpdate): {version.ToVersionString()}", sign, sign);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                _messenger.Error(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags AmendLastCommit(IRepository repo, GitVersion version, ConfigFile configFile)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = BuildSignature(repo, configFile);
                repo.Commit($"chore(VersionUpdate): {version.ToVersionString()}", sign, sign, new CommitOptions()
                {
                    AmendPreviousCommit = true,
                });

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                _messenger.Error(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static Signature BuildSignature(IRepository repo, ConfigFile configFile)
        {
            var signature = repo.Config.BuildSignature(DateTime.Now);

            return signature;
        }

        private ReleaseManagerFlags Stage(IRepository repo)
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
                _messenger.Error(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags CreateTag(IRepository repo, GitVersion version)
        {
            try
            {
                repo.ApplyTag($"v{version.ToVersionString()}");
                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                _messenger.Error(e);
                return ReleaseManagerFlags.TagCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdateChangelog(ReleaseChoices releaseChoices, ConfigFile configFile, GitVersion current, GitVersion nextVersion)
        {
            try
            {
                var changelogFileName = releaseChoices.ChangelogFileName ?? "CHANGELOG.md";

                string args = GetChangelogArgs(releaseChoices, configFile, current, nextVersion, changelogFileName);

                var (_, isError) = CommandExecutor.ExecuteCommand(GetCommandName(), args, _rootDir);
                var appendString = string.Empty;

                if (!isError)
                {
                    var changelogFilePath = Path.Combine(_rootDir, changelogFileName);

                    var changelogFileActualLimit = (int)releaseChoices.ChangelogCharacterLimit;

                    if (!string.IsNullOrEmpty(releaseChoices.AppendValue))
                    {
                        changelogFileActualLimit -= releaseChoices.AppendValue.Length + Environment.NewLine.Length;
                        appendString = Environment.NewLine + releaseChoices.AppendValue;
                    }

                    if (releaseChoices.ChangelogCharacterLimit > 0)
                    {
                        var readAllText = File.ReadAllText(changelogFilePath);
                        if (readAllText.Length > changelogFileActualLimit)
                        {
                            var limitedLength = readAllText.Substring(0, changelogFileActualLimit);
                            limitedLength += appendString;

                            File.WriteAllText(changelogFilePath, limitedLength);
                        }
                    }
                }

                return isError ? ReleaseManagerFlags.ChangelogCreationFailed : ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                _messenger.Error(ex);
                return ReleaseManagerFlags.ChangelogCreationFailed;
            }
        }

        private string GetCommandName()
        {
            return Path.Combine(_rootDir, "node_modules", ".bin", "changelog");
        }

        private static string GetChangelogArgs(
            ReleaseChoices releaseChoices,
            ConfigFile configFile,
            GitVersion current,
            GitVersion nextVersion,
            string changelogFileName)
        {
            var args =
                $"generate --file {changelogFileName}" + (releaseChoices.ChangeLogType == ChangeLogType.Incremental
                    ? $" --tag v{current}..v{nextVersion}"
                    : string.Empty);

            args = string.IsNullOrEmpty(configFile.ChangelogOption?.ExcludeType)
                ? args
                : args + $" --exclude {configFile.ChangelogOption.ExcludeType}";

            args = string.IsNullOrEmpty(configFile.ChangelogOption?.ProjectUrl)
                ? args
                : args + $" --repo-url {configFile.ChangelogOption.ProjectUrl}";

            return args;
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

        private ReleaseManagerFlags UpdatePackageVersion(GitVersion version, bool isNativeProject)
        {
            return _package.SetVersion(version, isNativeProject);
        }

        private GitVersion GetCurrentVersion(ReleaseChoices releaseChoices)
        {
            return releaseChoices.ReleaseType == ReleaseType.Custom
                ? releaseChoices.CustomVersion
                : _package.GetVersion();
        }

        private ReleaseManagerFlags IsRepoReadyForRelease(IRepository repo, ReleaseChoices releaseChoices)
        {
            if (IsRepoDirty(repo) && !releaseChoices.IgnoreDirty)
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

        private ReleaseManagerFlags ExecuteRepoSafe(bool isIgnoreDirty,
            Func<IRepository, ReleaseManagerFlags> func)
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
                var shouldResetRepo = result != ReleaseManagerFlags.Ok &&
                                      result != ReleaseManagerFlags.DirtyRepo &&
                                      !isIgnoreDirty;
                if (shouldResetRepo)
                {
                    repo.Reset(ResetMode.Hard, repoHead.Tip);
                    repo.RemoveUntrackedFiles();
                }
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags ExecuteSafe(Func<ReleaseManagerFlags> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                _messenger.Error(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        public IEnumerable<string> GetVersion(string platformName)
        {
            var list = new List<string> {GetCurrentVersion(new ReleaseChoices()).ToString()};

            return list.Union(platformName == "all"
                ? CreatePlatforms(_package.GetConfig().Platforms).Select(p => p.Value.GetVersion()).ToArray()
                : new[] { CreatePlatforms(_package.GetConfig().Platforms)[platformName].GetVersion() });
        }

        public ReleaseManagerFlags SetupRepo(bool generic)
        {
            return ExecuteSafe(() =>
            {
                return ExecuteRepoSafe(false, repo =>
                {
                    _messenger.Info("Staring Init sequence.");
                    var validityFlag = IsRepoReadyForSetup(repo);

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    _messenger.Info("Initializing Changelog.");
                    validityFlag = InitNpm();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InstallChangelogGenerator();

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    if (!generic)
                    {
                        _messenger.Info("Setting up dll version file.");
                        validityFlag = InitNbgv();
                    }

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    validityFlag = InitDefaultConfig(generic);

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
                CommandExecutor.ExecuteCommand("npm", "install generate-changelog@1.8.0", _rootDir);

            if (isError)
                return ReleaseManagerFlags.ChangelogGeneratorInstallFailed;

            (_, isError) =
                CommandExecutor.ExecuteCommand("npm", "install", _rootDir);

            return isError ? ReleaseManagerFlags.ChangelogGeneratorInstallFailed : ReleaseManagerFlags.Ok;
        }

        private ReleaseManagerFlags InitDefaultConfig(bool generic)
        {
            var save = new ConfigFile
            {
                IsGenericProject = generic,
                Platforms = new[]
                {
                    new Platform()
                    {
                        Name = "platform name, supported are [ios, droid, uwp]. or it can be left empty in case of simple dotnet project.",
                        Path = "path to the root of the specified platforms project."
                    }
                },
                ChangelogOption = new ChangelogOption
                {
                    ExcludeType = "chore"
                }
            }.Save(Path.Combine(_rootDir, ConfigFileName.FixName));

            return save ? ReleaseManagerFlags.Ok : ReleaseManagerFlags.ConfigFileCreationFailed;
        }

        private ReleaseManagerFlags InitNbgv()
        {
            var (_, isError) =
                CommandExecutor.ExecuteCommand("dotnet", "tool install --tool-path . nbgv --version 3.3.37", _rootDir);

            if (isError)
                return ReleaseManagerFlags.NBGVInstallationFailed;

            (_, isError) = CommandExecutor.ExecuteCommand("./nbgv", "install", _rootDir);

            return isError ? ReleaseManagerFlags.NBGVInitFailed : ReleaseManagerFlags.Ok;
        }

        private ReleaseManagerFlags InitNpm()
        {
            var (_, isError) = CommandExecutor.ExecuteCommand("npm", "init -f", _rootDir);

            return isError ? ReleaseManagerFlags.NPMInitFailed : ReleaseManagerFlags.Ok;
        }

        public void IncrementBuildVersion(string platform)
        {
            if (_package.IsInitialized())
            {
                var platforms = CreatePlatforms(_package.GetConfig().Platforms);

                if (platforms.ContainsKey(platform))
                {
                    var version = platforms[platform].GetVersion();
                    _messenger.Info(version);
                }
            }
        }
    }

    public interface IMessenger
    {
        void Info(string message);
        void Error(Exception exception);
    }
}
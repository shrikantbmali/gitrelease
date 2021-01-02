using System;

namespace gitrelease.core
{
    public class GitVersion
    {
        private static char Separator = '.';
        private static char PrerepeaseSeparator = '-';

        public string Patch { get; }

        public string Minor { get; }

        public string Major { get; }

        public string PreReleaseTag { get; private set; }

        public static GitVersion Empty = new GitVersion(string.Empty, string.Empty, string.Empty);

        //public string CommitId { get; }

        private GitVersion(string major, string minor, string patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        private GitVersion(string major, string minor, string patch, string gitSha)
            : this(major, minor, patch)
        {
            //CommitId = gitSha;
        }

        public GitVersion(GitVersion gitVersion, string commitSha):
            this(gitVersion.Major, gitVersion.Minor, gitVersion.Patch, commitSha)
        {
        }

        public GitVersion(GitVersion gitVersion):
            this(gitVersion.Major, gitVersion.Minor, gitVersion.Patch)
            //this(gitVersion.Major, gitVersion.Minor, gitVersion.Patch, gitVersion.CommitId)
        {
            PreReleaseTag = gitVersion.PreReleaseTag;
        }

        public GitVersion(string version)
        {
            if(version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            var versions = version.Split(Separator);

            if (versions.Length != 3)
                throw new ArgumentException(nameof(versions) + " is not in {major}.{minor}.{patch} format");

            Major = versions[0];
            Minor = versions[1];
            Patch = GetPatch(versions[2]);
            PreReleaseTag = GetPrereleaseTag(versions[2]);
        }

        public string ToVersionString()
        {
            var versionString = $"{Major}.{Minor}.{Patch}";

            if (IsPrerelease())
            {
                versionString += $"-{PreReleaseTag}";
            }

            return versionString;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}" + (IsPrerelease() ? $"-{PreReleaseTag}" : string.Empty);
        }

        public GitVersion IncrementMajorAndGetNew()
        {
            return new GitVersion((int.Parse(Major) + 1).ToString(), Minor, Patch);
        }

        public GitVersion IncrementMinorAndGetNew()
        {
            return new GitVersion(Major, (int.Parse(Minor) + 1).ToString(), Patch);
        }

        public GitVersion IncrementPatchAndGetNew()
        {
            return new GitVersion(Major, Minor, (int.Parse(Patch) + 1).ToString());
        }

        public GitVersion GetNewWithPreReleaseTag(string preReleaseTag)
        {
            return new GitVersion(this)
            {
                PreReleaseTag = preReleaseTag
            };
        }

        public static bool IsValid(string version)
        {
            return !string.IsNullOrEmpty(version) && TryParse(version, out _);
        }

        private static bool TryParse(string version, out GitVersion gitVersion)
        {
            gitVersion = null;

            if (version == null) throw new ArgumentNullException(nameof(version));

            var splits = version.Split(Separator);

            if (splits.Length != 3)
                return false;

            gitVersion = new GitVersion(splits[0], splits[1], splits[2]);

            return true;
        }

        public static GitVersion Parse(string version)
        {
            TryParse(version, out var gitVersion);
            return gitVersion;
        }

        public static GitVersion Parse(string version, string preReleaseTag) =>
            Parse(version).GetNewWithPreReleaseTag(preReleaseTag);

        private static string GetPrereleaseTag(string version)
        {
            var strings = version.Split(PrerepeaseSeparator);

            if (strings.Length == 2)
            {
                return strings[1];
            }

            return string.Empty;
        }

        private static string GetPatch(string version)
        {
            var strings = version.Split(PrerepeaseSeparator);

            return strings[0];
        }

        public bool IsPrerelease()
        {
            return !string.IsNullOrEmpty(PreReleaseTag);
        }

        public static GitVersion GetPrerelease(string prerelease)
        {
            return new GitVersion("", "", "")
            {
                PreReleaseTag = prerelease
            };
        }
    }
}
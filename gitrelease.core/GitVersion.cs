using System;
using System.Globalization;

namespace gitrelease.core
{
    public class GitVersion
    {
        private const char Separator = '.';
        private const char PreReleaseSeparator = '-';

        public string Patch { get; }

        public string Minor { get; }

        public string Major { get; }

        public string PreReleaseTag { get; private set; }

        public string BuildNumber { get; private set; }

        public static GitVersion Empty = new GitVersion(string.Empty, string.Empty, string.Empty);

        private GitVersion(string major, string minor, string patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public GitVersion(GitVersion gitVersion):
            this(gitVersion.Major, gitVersion.Minor, gitVersion.Patch)
        {
            PreReleaseTag = gitVersion.PreReleaseTag;
            BuildNumber = gitVersion.BuildNumber;
        }

        public GitVersion(string version) : this(Parse(version))
        {
        }

        public string ToVersionString()
        {
            var versionString = $"{Major}{Separator}{Minor}{Separator}{Patch}";

            if (IsPreRelease())
            {
                versionString += $"{PreReleaseSeparator}{PreReleaseTag}";
            }

            if (!string.IsNullOrEmpty(BuildNumber))
            {
                versionString += $"{Separator}{BuildNumber}";
            }

            return versionString;
        }

        public string ToVersionStringV()
        {
            return $"v{ToVersionString()}";
        }

        public override string ToString()
        {
            return ToVersionString();
        }

        public GitVersion IncrementMajor()
        {
            return new GitVersion((int.Parse(Major) + 1).ToString(), Minor, Patch)
            {
                BuildNumber = this.BuildNumber,
                PreReleaseTag = this.PreReleaseTag
            };
        }

        public GitVersion IncrementMinor()
        {
            return new GitVersion(Major, (int.Parse(Minor) + 1).ToString(), Patch)
            {
                BuildNumber = this.BuildNumber,
                PreReleaseTag = this.PreReleaseTag
            };
        }

        public GitVersion IncrementPatch()
        {
            return new GitVersion(Major, Minor, (int.Parse(Patch) + 1).ToString())
            {
                BuildNumber = this.BuildNumber,
                PreReleaseTag = this.PreReleaseTag
            };
        }

        public GitVersion ResetMinor()
        {
            return new GitVersion(Major, 0.ToString(), Patch)
            {
                BuildNumber = this.BuildNumber,
                PreReleaseTag = this.PreReleaseTag
            };
        }

        public GitVersion ResetPatch()
        {
            return new GitVersion(Major, Minor, 0.ToString())
            {
                BuildNumber = this.BuildNumber,
                PreReleaseTag = this.PreReleaseTag
            };
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

        public static GitVersion Parse(string version)
        {
            TryParse(version, out var gitVersion);
            return gitVersion;
        }

        private static bool TryParse(string version, out GitVersion gitVersion)
        {
            gitVersion = null;

            if (version == null) throw new ArgumentNullException(nameof(version));

            var splits = version.Split(Separator);

            if (splits.Length < 3 || splits.Length > 4)
                return false;

            var patch = splits[2];

            var patchSplit = patch.Split('-');
            var preRelease = string.Empty;

            if (patchSplit.Length == 2)
            {
                patch = patchSplit[0];
                preRelease = patchSplit[1];
            }

            gitVersion = new GitVersion(splits[0], splits[1], patch)
            {
                BuildNumber = splits.Length == 4 ? splits[3] : null,
                PreReleaseTag = preRelease
            };

            return true;
        }

        public static GitVersion Parse(string version, string preReleaseTag) =>
            Parse(version).GetNewWithPreReleaseTag(preReleaseTag);

        public bool IsPreRelease()
        {
            return !string.IsNullOrEmpty(PreReleaseTag);
        }

        public static GitVersion GetPrerelease(string preRelease)
        {
            return new GitVersion("0", "0", "0")
            {
                PreReleaseTag = preRelease
            };
        }

        public GitVersion SetBuildNumberAndGetNew(string buildNumber)
        {
            return new GitVersion(this)
            {
                BuildNumber = buildNumber
            };
        }
    }
}
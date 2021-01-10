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
            var versionString = $"{Major}.{Minor}.{Patch}";

            if (IsPreRelease())
            {
                versionString += $"-{PreReleaseTag}";
            }

            if (!string.IsNullOrEmpty(BuildNumber))
            {
                versionString += $".{BuildNumber}";
            }

            return versionString;
        }

        public override string ToString()
        {
            return ToVersionString();
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

            gitVersion = new GitVersion(splits[0], splits[1], splits[2])
            {
                BuildNumber = splits.Length == 4 ? splits[3] : null
            };

            return true;
        }

        public static GitVersion Parse(string version, string preReleaseTag) =>
            Parse(version).GetNewWithPreReleaseTag(preReleaseTag);

        private static string GetPreReleaseTag(string version)
        {
            var strings = version.Split(PreReleaseSeparator);

            if (strings.Length == 2)
            {
                return strings[1];
            }

            return string.Empty;
        }

        private static string GetPatch(string version)
        {
            var strings = version.Split(PreReleaseSeparator);

            return strings[0];
        }

        public bool IsPreRelease()
        {
            return !string.IsNullOrEmpty(PreReleaseTag);
        }

        public static GitVersion GetPrerelease(string preRelease)
        {
            return new GitVersion("", "", "")
            {
                PreReleaseTag = preRelease
            };
        }

        public GitVersion SetBuildNumberAndGetNew(int buildNumber)
        {
            return new GitVersion(this)
            {
                BuildNumber = buildNumber.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
using System;

namespace gitrelease.core
{
    internal class GitVersion
    {
        private static char Separator = '.';

        public string Patch { get; }

        public string Minor { get; }

        public string Major { get; }

        public string CommitId { get; }

        private GitVersion(string major, string minor, string patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
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
            Patch = versions[2];
        }

        public GitVersion(GitVersion gitVersion, string commitSha) : 
            this(gitVersion.Major, gitVersion.Minor, gitVersion.Patch)
        {
            CommitId = commitSha;
        }

        public string ToMajorMinorPatch()
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{CommitId}";
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
    }
}
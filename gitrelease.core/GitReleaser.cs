using gitrelease.core.nvgb;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;

namespace gitrelease
{
    internal static class GitReleaser
    {
        internal static ReleaseSequenceFlags PrepareRelease(Repository repo, Func<string, ReleaseSequenceFlags> version)
        {
            try
            {
                var rootDirectory = Path.GetDirectoryName(Path.GetDirectoryName(repo.Info.Path));
                string command = Path.Combine(rootDirectory, @".\nbgv.exe");
                return ExecuteCommand(command, $"prepare-release -p {rootDirectory} -f json",
                    output => DetermineVersion(output, version));
            }
            catch (Exception ex)
            {
                return ReleaseSequenceFlags.Unknown;
            }

            return ReleaseSequenceFlags.Ok;
        }

        private static ReleaseSequenceFlags DetermineVersion(string output, Func<string, ReleaseSequenceFlags> func)
        {
            try
            {
                var releaseMessage = ReleaseMessage.FromJson(output);
                
                if (releaseMessage?.NewBranch?.Version != null)
                {
                    
                    var version = $"{releaseMessage.NewBranch.Version}.{releaseMessage.NewBranch.Commit.Substring(0, 10)}";
                    return func(version);
                }
                else
                    return ReleaseSequenceFlags.Unknown;
            }
            catch (Exception ex)
            {
                return ReleaseSequenceFlags.Unknown;
            }

            return ReleaseSequenceFlags.Unknown;
        }

        private static ReleaseSequenceFlags ExecuteCommand(string command, string args, Func<string, ReleaseSequenceFlags> callback)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();

            return callback(string.IsNullOrEmpty(err) ? output : err);
        }
    }
}

namespace gitrelease.core.nvgb
{
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ReleaseMessage
    {
        [JsonProperty("CurrentBranch")]
        public Branch CurrentBranch { get; set; }

        [JsonProperty("NewBranch")]
        public Branch NewBranch { get; set; }
    }

    public partial class Branch
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Commit")]
        public string Commit { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }
    }

    public partial class ReleaseMessage
        {
        public static ReleaseMessage FromJson(string json) => JsonConvert.DeserializeObject<ReleaseMessage>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ReleaseMessage self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

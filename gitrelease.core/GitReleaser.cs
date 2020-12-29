using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace gitrelease.core
{
    internal static class GitReleaser
    {
        internal static ReleaseManagerFlags PrepareRelease(Repository repo, Func<string, ReleaseManagerFlags> version)
        {
            var head = repo.Head;

            try
            {
                var rootDirectory = Path.GetDirectoryName(Path.GetDirectoryName(repo.Info.Path));
                string command = Path.Combine(rootDirectory, @".\nbgv.exe");
                return CommandExecutor.ExecuteCommand(command, $"prepare-release -p {rootDirectory} -f json",
                    output => DetermineVersion(output, version));
            }
            catch (Exception ex)
            {
                repo.Reset(ResetMode.Hard, head.Tip);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags DetermineVersion(string output, Func<string, ReleaseManagerFlags> func)
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
                    return ReleaseManagerFlags.Unknown;
            }
            catch (Exception ex)
            {
                return ReleaseManagerFlags.Unknown;
            }
        }
    }

    internal static class CommandExecutor
    {
        public static ReleaseManagerFlags ExecuteCommand(string command, string args, Func<string, ReleaseManagerFlags> callback)
        {
            var process = new Process
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

    internal partial class ReleaseMessage
    {
        [JsonProperty("CurrentBranch")]
        public Branch CurrentBranch { get; set; }

        [JsonProperty("NewBranch")]
        public Branch NewBranch { get; set; }
    }

    internal class Branch
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Commit")]
        public string Commit { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }
    }

    internal partial class ReleaseMessage
    {
        public static ReleaseMessage FromJson(string json) => JsonConvert.DeserializeObject<ReleaseMessage>(json, Converter.Settings);
    }

    internal static class Serialize
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
using System;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace gitrelease.core
{
    public static class CommandExecutor
    {
        public static (string output, bool isError) ExecuteFile(string command, string args)
        {
            try
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

                return (string.IsNullOrEmpty(err) ? output : err, !string.IsNullOrEmpty(err));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (string.Empty, true);
            }
        }

        public static (string output, bool isError) ExecuteCommand(string command, string args, string workingDirectory)
        {
            try
            {
                var procStartInfo = new ProcessStartInfo("cmd", "/c " + command + " " + string.Join(' ', args))
                {
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = new Process {StartInfo = procStartInfo};
                process.Start();

                process.WaitForExit();

                var output = process?.StandardOutput.ReadToEnd();
                var err = process?.StandardError.ReadToEnd();

                Console.WriteLine($"Out : {output} \n Err {err}");

                return (process.ExitCode >= 0 ? output : err, process.ExitCode < 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (string.Empty, true);
            }
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
        public static ReleaseMessage FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ReleaseMessage>(json, Converter.Settings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
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
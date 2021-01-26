using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
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

                return IsError(process);
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
                var procStartInfo = new ProcessStartInfo(GetCommandExecutor(), command + " " + string.Join(' ', args))
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

                return IsError(process);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (string.Empty, true);
            }
        }

        private static string GetCommandExecutor()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd /c " : "bin/bash";
        }

        private static (string output, bool isError) IsError(Process process)
        {
            var output = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();

            var isError = process.ExitCode != 0;
            return (isError ? output : err, isError);
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
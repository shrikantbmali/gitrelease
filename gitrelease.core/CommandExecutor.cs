using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ToolBox.Bridge;
using ToolBox.Notification;
using ToolBox.Platform;

namespace gitrelease.core
{
    public static class CommandExecutor
    {
        private static ShellConfigurator _shell;

        static CommandExecutor()
        {
            _shell = new ShellConfigurator(GetBridge(), NotificationSystem.Default);
        }

        private static IBridgeSystem GetBridge()
        {
            switch (OS.GetCurrent())
            {
                case "win":
                    return BridgeSystem.Bat;
                case "mac":
                case "gnu":
                    return  BridgeSystem.Bash;
            }

            return BridgeSystem.Bat;
        }

        public static (string output, bool isError) ExecuteCommand(string command, string args, string workingDirectory)
        {
            try
            {
                var response = _shell.Term($"{command} {string.Join(' ', args)}", Output.Internal, workingDirectory);
                return IsError(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (string.Empty, true);
            }
        }
        
        private static (string output, bool isError) IsError(Response process)
        {
            var isError = process.code != 0;
            return (isError ? process.stdout : process.stderr, isError);
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
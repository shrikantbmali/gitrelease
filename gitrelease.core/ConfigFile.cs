using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gitrelease
{
    public struct ConfigFile
    {
        [JsonPropertyName("platforms")]
        public IEnumerable<Platform> Platforms { get; set; }

        [JsonIgnore]
        public const string FixName = "gitrelease.config.json";
    }

    public struct Platform
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}
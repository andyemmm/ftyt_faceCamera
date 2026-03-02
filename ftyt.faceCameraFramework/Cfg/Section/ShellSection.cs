using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.Cfg.Section
{
    public class ShellSection
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "WELCOME";
        [JsonPropertyName("content")] public string Content { get; set; } = "WELCOME";
        [JsonPropertyName("useChineseEnumDescriptions")] public bool UseChineseEnumDescriptions { get; set; } = true;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.Cfg.Section
{
    public class ErrorSection
    {
        [JsonPropertyName("content")] public string Content { get; set; } = "Our Staff Will Be With You Shortly To Provide Assistance";
        [JsonPropertyName("duration")] public int? Duration { get; set; } = 5;
    }
}

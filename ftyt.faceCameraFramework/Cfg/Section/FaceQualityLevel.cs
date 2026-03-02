using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.Cfg.Section
{
    public class FaceQualityLevel
    {
        [JsonPropertyName("half")] public int Half { get; set; } = 0;
        [JsonPropertyName("small")] public int Small { get; set; } = 0;
        [JsonPropertyName("posture")] public int Posture { get; set; } = 0;
        [JsonPropertyName("mask")] public int Mask { get; set; } = 0;
        [JsonPropertyName("faceMask")] public int FaceMask { get; set; } = 0;
        [JsonPropertyName("hat")] public int Hat { get; set; } = 0;
        [JsonPropertyName("glasses")] public int Glasses { get; set; } = 0;
        [JsonPropertyName("gape")] public int Gape { get; set; } = 0;
        [JsonPropertyName("blur")] public int Blur { get; set; } = 0;
        [JsonPropertyName("bright")] public int Bright { get; set; } = 0;
        [JsonPropertyName("light")] public int Light { get; set; } = 0;
    }
}

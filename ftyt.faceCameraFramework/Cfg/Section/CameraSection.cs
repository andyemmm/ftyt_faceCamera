using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.Cfg.Section
{
    public class CameraSection
    {
        [JsonPropertyName("deviceNo")] public int DeviceNo { get; set; } = 0;
        [JsonPropertyName("detectInterval")] public int DetectInterval { get; set; } = 1000;
        [JsonPropertyName("liveScore")] public int LiveScore { get; set; } = 92;
        [JsonPropertyName("captureSuccessDuration")] public int CaptureSuccessDuration { get; set; } = 5000;
        [JsonPropertyName("IREnable")] public bool IREnable { get; set; } = true;
        [JsonPropertyName("LiveEnable")] public bool LiveEnable { get; set; } = true;
    }
}

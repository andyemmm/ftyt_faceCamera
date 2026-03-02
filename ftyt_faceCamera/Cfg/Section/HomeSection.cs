using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Cfg.Section
{
    public class HomeSection
    {
        [JsonPropertyName("faceCaptureTimeout")] public string FaceCaptureTimeout { get; set; } = "Face Capture Timeout";
        [JsonPropertyName("faceCaptureTimeoutDuration")] public int FaceCaptureTimeoutDuration { get; set; } = 2;
        [JsonPropertyName("faceMatchFail")] public string FaceMatchFail { get; set; } = "Face Match Fail";
        [JsonPropertyName("processing")] public string Processing { get; set; } = "Processing";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Cfg.Section
{
    public class CameraSection
    {
        [JsonPropertyName("deviceNo")] public int DeviceNo { get; set; } = 0;
    }
}

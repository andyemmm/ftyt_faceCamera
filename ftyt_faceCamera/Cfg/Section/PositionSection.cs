using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Cfg.Section
{
    public class PositionSection
    {
        [JsonPropertyName("top")] public double Top { get; set; }
        [JsonPropertyName("left")] public double Left { get; set; }
    }
}

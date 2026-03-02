using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Cfg.Section
{
    public class LayoutSection
    {
        [JsonPropertyName("scale")] public double Scale { get; set; } = 1;
        [JsonPropertyName("position")] public PositionSection Position { get; set; } = new PositionSection();
        [JsonPropertyName("topmost")] public bool Topmost { get; set; }
        [JsonPropertyName("removeFrame")] public bool RemoveFrame { get; set; }
        [JsonPropertyName("maximize")] public bool Maximize { get; set; }
        [JsonPropertyName("setForeground")] public bool SetForeground { get; set; }

    }
}

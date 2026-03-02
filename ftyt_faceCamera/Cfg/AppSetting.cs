using ftyt_faceCamera.Cfg.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Cfg
{
    public class AppSetting
    {
        [JsonPropertyName("layout")] public LayoutSection Layout { get; set; } = new LayoutSection();
        [JsonPropertyName("shell")] public ShellSection Shell { get; set; } = new ShellSection();
        [JsonPropertyName("home")] public HomeSection Home { get; set; } = new HomeSection();
        [JsonPropertyName("error")] public ErrorSection Error { get; set; } = new ErrorSection();
        [JsonPropertyName("camera")] public CameraSection Camera { get; set; } = new CameraSection();
    }
}

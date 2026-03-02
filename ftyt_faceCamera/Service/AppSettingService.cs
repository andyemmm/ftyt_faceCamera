using ftyt_faceCamera.Cfg;
using ftyt_faceCamera.IService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Service
{
    public class AppSettingService<T> where T : AppSetting, new()
    {
        public AppSettingService(ILogService logService)
        {
            var path = SettingFilePath;
            if (File.Exists(path))
            {
                try
                {
                    Content = JsonSerializer.Deserialize<T>(File.ReadAllText(path));
                    OnSettingLoaded();
                }
                catch (Exception exception)
                {
                    logService.Error("[AppSettingService] unable to load appSetting.json", exception);
                    Content = new T();
                }
            }
            else
            {
                logService.Warn("appSetting.json is not exists");
                Content = new T();
            }
        }

        string SettingFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "appSetting.json");

        public T Content { get; }

        public void SaveSetting()
        {
            var json = JsonSerializer.Serialize(Content, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(SettingFilePath, json);
        }

        protected virtual void OnSettingLoaded() { }
    }
}

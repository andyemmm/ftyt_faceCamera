using ftyt_faceCamera.Cfg;
using ftyt_faceCamera.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Service
{

    public class DemoAppSettingService : AppSettingService<AppSetting>
    {
        public DemoAppSettingService(ILogService logService) : base(logService)
        {

        }

        protected override void OnSettingLoaded()
        {
            base.OnSettingLoaded();
        }
    }
}

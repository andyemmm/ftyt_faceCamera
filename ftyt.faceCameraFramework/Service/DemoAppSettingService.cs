using ftyt.faceCameraFramework.Cfg;
using ftyt.faceCameraFramework.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.Service
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

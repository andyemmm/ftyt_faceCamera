using ftyt.faceCameraFramework.IService;
using ftyt.faceCameraFramework.Service;
using ftyt.faceCameraFramework.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace ftyt.faceCameraFramework
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        readonly LogService _logService;

        public App() : base()
        {
            _logService = new LogService();
        }

        protected override Window CreateShell()
        {
            var view = Container.Resolve<ShellView>();
            return view;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<ILogService>(_logService);
            var container = Container.GetContainer();

            containerRegistry.RegisterInstance<IThFaceService>(container.Resolve<ThFaceService>());
            containerRegistry.RegisterInstance<ICaptureService>(container.Resolve<CaptureService>());
            containerRegistry.RegisterInstance<IDialogService>(container.Resolve<DialogService>());
        }
    }
}

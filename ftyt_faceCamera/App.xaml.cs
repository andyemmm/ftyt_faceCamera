using ftyt_faceCamera.IService;
using ftyt_faceCamera.Service;
using ftyt_faceCamera.Views;
using Prism.Dialogs;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using DialogService = ftyt_faceCamera.Service.DialogService;
using IDialogService = ftyt_faceCamera.IService.IDialogService;

namespace ftyt_faceCamera;

/// <summary>
/// Interaction logic for App.xaml
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


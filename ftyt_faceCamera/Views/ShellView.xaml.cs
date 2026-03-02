using ftyt_faceCamera.Extension;
using ftyt_faceCamera.Service;
using ftyt_faceCamera.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ftyt_faceCamera.Views
{
    /// <summary>
    /// ShellView.xaml 的交互逻辑
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView(DemoAppSettingService appSettingService)
        {
            //InitializeComponent();
            //this.UseLayout(scaleTransform, appSettingService.Content.Layout);
            ////this.Closing += async (sender, e) => await eyecoolCamService.TurnOffAsync();

            InitializeComponent();
            scaleTransform.ScaleX = scaleTransform.ScaleY = appSettingService.Content.Layout.Scale;
            Top = appSettingService.Content.Layout.Position.Top;
            Left = appSettingService.Content.Layout.Position.Left;
            if (appSettingService.Content.Layout.Topmost)
            {
                Topmost = true;
                WindowStyle = WindowStyle.None;
            }

            this.Closing += ShellView_Closing;
        }

        private void ShellView_Closing(object? sender, CancelEventArgs e)
        {
            this.Closing += async (s, e) =>
            {
                if (DataContext is ShellViewModel vm)
                {
                    // 阻止默认关闭，等待资源释放
                    e.Cancel = true;
                    await vm.OnApplicationExitAsync();
                    this.Close();
                }
            };
        }
    }
}

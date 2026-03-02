using ftyt.faceCameraFramework.Cfg.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;

namespace ftyt.faceCameraFramework.Extensions
{
    public static class WindowExtension
    {
        public static void UseLayout(this Window window, ScaleTransform scaleTransform, LayoutSection layout)
        {
            scaleTransform.ScaleX = scaleTransform.ScaleY = layout.Scale;
            window.ResizeMode = ResizeMode.NoResize;
            if (layout.RemoveFrame) window.WindowStyle = WindowStyle.None;
            if (layout.Maximize) window.WindowState = WindowState.Maximized;
            if (layout.Topmost) window.Topmost = true;
            if (layout.SetForeground)
            {
                Task.Factory.StartNew(async () => {
                    IntPtr hWnd = IntPtr.Zero;
                    while (true)
                    {
                        try
                        {
                            await Task.Delay(3000);

                            if (hWnd == IntPtr.Zero)
                            {
                                var hwndSource = HwndSource.FromVisual(window) as HwndSource;
                                if (hwndSource != null) hWnd = hwndSource.Handle;
                            }
                            if (hWnd != IntPtr.Zero)
                            {
                                Application.Current.Dispatcher.Invoke(() => {
                                    if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Maximized;
                                    SetForegroundWindow(hWnd);
                                });
                            }
                        }
                        catch { }
                    }
                });
            }

            window.Top = layout.Position.Top;
            window.Left = layout.Position.Left;
        }


        #region P/Invoke

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetForegroundWindow(IntPtr hWnd);

        #endregion
    }
}

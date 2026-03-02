using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ftyt_faceCamera.IService;

namespace ftyt_faceCamera.Service
{
    // 相机取帧及帧图象显示组件
    public class CaptureService : ICaptureService
    {
        // 组件初始化，返回组件对象
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_INIT", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CHS_CAPTURE_INIT();

        // 打开相机（根据相机索引号 nDeviceId）
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_OPEN", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_OPEN(IntPtr hInstance, int nDeviceId);

        // 从打开的相机提取一帧视频数据，nWidth * nHeight 为视频图象的分辨率（宽 * 高）
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_GETFRAME", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_GETFRAME(IntPtr hInstance, IntPtr pFrameBuf, ref int nLen, ref int nWidth, ref int nHeight);

        // 显示视频帧图象，nWidth * nHeight 为视频帧图象的分辨率（宽 * 高），nChannel 为图象数据的位深度（如传入24，表示传入的是24位图象数据）
        // 特别注意调用后输入的图象数据被倒转了（头朝上的图象数据变成了头朝下），为避免数据改变后其它地方再使用帧数据出现问题,，建议将帧数据拷贝到临时缓冲区来调用
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_DRAWFACEFRAME", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_DRAWFACEFRAME(IntPtr hInstance, IntPtr hWND, IntPtr pFrame, int nWidth, int nHeight, int nChannal, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBottom);

        // 保存人脸图象，特别注意要求输入的图象数据 pFrame 是倒立的（头朝下脚朝上），但人脸坐标（nFaceLeft, nFaceTop, nFaceRight, nFaceBottom）仍相对于正立图象
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_SAVEBMPFILE", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_SAVEBMPFILE(IntPtr filename, IntPtr pFrame, int nWidth, int nHeight, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBotton);

        // 关闭相机
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_CLOSE", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_CLOSE(IntPtr hInstance);

        // 组件反初始化，释放资源
        [DllImport("CHS_CAPTURE.dll", EntryPoint = "CHS_CAPTURE_UNINIT", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CHS_CAPTURE_UNINIT(IntPtr hInstance);


        // 组件初始化，返回组件对象
        public nint CAPTURE_INIT()
        {
            return CHS_CAPTURE_INIT();
        }

        // 打开相机（根据相机索引号 nDeviceId）
        public int CAPTURE_OPEN(nint hInstance, int nDeviceId)
        {
            return CHS_CAPTURE_OPEN(hInstance, nDeviceId);
        }

        // 从打开的相机提取一帧视频数据，nWidth * nHeight 为视频图象的分辨率（宽 * 高）
        public int CAPTURE_GETFRAME(nint hInstance, nint pFrameBuf, ref int nLen, ref int nWidth, ref int nHeight)
        {
            return CHS_CAPTURE_GETFRAME(hInstance, pFrameBuf, ref nLen, ref nWidth, ref nHeight);
        }

        // 显示视频帧图象，nWidth * nHeight 为视频帧图象的分辨率（宽 * 高），nChannel 为图象数据的位深度（如传入24，表示传入的是24位图象数据）
        // 特别注意调用后输入的图象数据被倒转了（头朝上的图象数据变成了头朝下），为避免数据改变后其它地方再使用帧数据出现问题,，建议将帧数据拷贝到临时缓冲区来调用
        public int CAPTURE_DRAWFACEFRAME(nint hInstance, nint hWND, nint pFrame, int nWidth, int nHeight, int nChannal, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBottom)
        {
            return CHS_CAPTURE_DRAWFACEFRAME(hInstance, hWND, pFrame, nWidth, nHeight, nChannal, nFaceLeft, nFaceTop, nFaceRight, nFaceBottom);
        }

        // 保存人脸图象，特别注意要求输入的图象数据 pFrame 是倒立的（头朝下脚朝上），但人脸坐标（nFaceLeft, nFaceTop, nFaceRight, nFaceBottom）仍相对于正立图象
        public int CAPTURE_SAVEBMPFILE(nint filename, nint pFrame, int nWidth, int nHeight, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBotton)
        {
            return CHS_CAPTURE_SAVEBMPFILE(filename, pFrame, nWidth, nHeight, nFaceLeft, nFaceTop, nFaceRight, nFaceBotton);
        }

        // 关闭相机
        public int CAPTURE_CLOSE(nint hInstance)
        {
            return CHS_CAPTURE_CLOSE(hInstance);
        }

        // 组件反初始化，释放资源
        public int CAPTURE_UNINIT(nint hInstance)
        {
            return CHS_CAPTURE_UNINIT(hInstance);
        }
    }
}

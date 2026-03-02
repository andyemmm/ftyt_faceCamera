using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt_faceCamera.IService
{
    public interface ICaptureService
    {
        IntPtr CAPTURE_INIT();
        int CAPTURE_OPEN(IntPtr hInstance, int nDeviceId);
        int CAPTURE_GETFRAME(IntPtr hInstance, IntPtr pFrameBuf, ref int nLen, ref int nWidth, ref int nHeight);
        int CAPTURE_DRAWFACEFRAME(IntPtr hInstance, IntPtr hWND, IntPtr pFrame, int nWidth, int nHeight, int nChannal, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBottom);
        int CAPTURE_SAVEBMPFILE(IntPtr filename, IntPtr pFrame, int nWidth, int nHeight, int nFaceLeft, int nFaceTop, int nFaceRight, int nFaceBotton);
        int CAPTURE_CLOSE(IntPtr hInstance);
        int CAPTURE_UNINIT(IntPtr hInstance);
    }
}

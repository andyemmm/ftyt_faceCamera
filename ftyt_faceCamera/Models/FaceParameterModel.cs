using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Models
{
    public class FaceParameterModel
    {

        #region 结构体定义

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        };

        // 人脸检测返回的人脸坐标参数
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct FACE_DETECT_RESULT
        {
            public RECT rcFace;//coordinate of face
            public POINT ptLeftEye;//coordinate of left eye
            public POINT ptRightEye;//coordinate of right eye
            public POINT ptMouth;//coordinate of mouth
            public POINT ptNose;//coordinate of nose								
            public Int32 nAngleYaw, nAnglePitch, nAngleRoll;//value of face angle
            public Int32 nQuality;//quality of face(from 0 to 100)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public Byte[] FaceData;
        };

        // 人脸质量检测返回的人脸质量参数
        public struct FACE_QUALITY_LEVEL
        {
            public Int32 nHalf; // 人脸完整性: 0-正常，1-人脸不太完整，2-人脸非常不完整
            public Int32 nSmall; // 人脸大小：0-正常，1-人脸较小，2-人脸太小
            public Int32 nPosture; // 姿态：0-正常，1-偏头较多，2-偏头太多
            public Int32 nMask; // 脸部遮挡: 0-正常，1-人脸有遮挡，2-人脸遮挡太多
            public Int32 nFaceMask; // 口罩：0-正常，1-有戴口罩，2-确认戴口罩
            public Int32 nHat; // 帽子：0-正常，1-有戴帽，2-帽子遮挡脸部
            public Int32 nGlasses; // 眼镜: 0-正常，1-有戴眼镜，2-确认戴眼镜
            public Int32 nGape; // 张嘴: 0-正常，1-张嘴，2-张大嘴
            public Int32 nBlur; // 模糊度：0-正常，1-较模糊，2-太模糊
            public Int32 nBright; // 脸部曝光度：0-正常，1-太暗，2-过爆
            public Int32 nLight; // 光源方向: 0-正常，1-侧光，2-顶光, 3-逆光       
        };

        #endregion

        public class DisplayFaceQualityLevelItem
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public int IntValue { get; set; }
        }
    }
}

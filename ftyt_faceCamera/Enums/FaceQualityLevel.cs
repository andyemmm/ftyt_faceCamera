using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Enums
{
    public class FaceQualityLevel
    {
        public enum FaceIntegrity
        {
            [Description("正常")] Normal = 0,
            [Description("人脸不太完整")] NotComplete = 1,
            [Description("人脸非常不完整")] VeryIncomplete = 2
        }

        public enum FaceSize
        {
            [Description("正常")] Normal = 0,
            [Description("人脸较小")] Small = 1,
            [Description("人脸太小")] TooSmall = 2
        }

        public enum FacePosture
        {
            [Description("正常")] Normal = 0,
            [Description("偏头较多")] HeadTurned = 1,
            [Description("偏头太多")] HeadTooTurned = 2
        }

        public enum FaceMaskOcclusion
        {
            [Description("正常")] Normal = 0,
            [Description("人脸有遮挡")] Occluded = 1,
            [Description("遮挡严重")] HeavilyOccluded = 2
        }

        public enum FaceMask
        {
            [Description("正常")] Normal = 0,
            [Description("有戴口罩")] WearingMask = 1,
            [Description("确认戴口罩")] MaskConfirmed = 2
        }

        public enum FaceHat
        {
            [Description("正常")] Normal = 0,
            [Description("有戴帽")] WearingHat = 1,
            [Description("帽子遮挡脸部")] HatOccludesFace = 2
        }

        public enum FaceGlasses
        {
            [Description("正常")] Normal = 0,
            [Description("有戴眼镜")] WearingGlasses = 1,
            [Description("确认戴眼镜")] GlassesConfirmed = 2
        }

        public enum FaceGape
        {
            [Description("正常")] Normal = 0,
            [Description("张嘴")] MouthOpen = 1,
            [Description("张大嘴")] MouthWideOpen = 2
        }

        public enum FaceBlur
        {
            [Description("正常")] Normal = 0,
            [Description("较模糊")] Blurry = 1,
            [Description("太模糊")] VeryBlurry = 2
        }

        public enum FaceBrightness
        {
            [Description("正常")] Normal = 0,
            [Description("太暗")] TooDark = 1,
            [Description("过曝")] Overexposed = 2
        }

        public enum FaceLightDirection
        {
            [Description("正常")] Normal = 0,
            [Description("侧光")] SideLight = 1,
            [Description("顶光")] TopLight = 2,
            [Description("逆光")] BackLight = 3
        }
    }
}

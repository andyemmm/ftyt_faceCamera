using ftyt_faceCamera.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ftyt_faceCamera.Enums.FaceQualityLevel;
using static ftyt_faceCamera.Models.FaceParameterModel;

namespace ftyt_faceCamera.Converters
{
    public class FaceQualityEnumConverter
    {
        private readonly bool _useChinese;

        public FaceQualityEnumConverter(bool useChinese)
        {
            _useChinese = useChinese;
        }

        public List<DisplayFaceQualityLevelItem> Convert(FACE_QUALITY_LEVEL level)
        {
            var result = new List<DisplayFaceQualityLevelItem>
        {
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "人脸完整性" :"Half",
                Value = EnumHelper.GetDescription((FaceIntegrity)level.nHalf, _useChinese),
                IntValue = level.nHalf
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "人脸大小" : "Small",
                Value = EnumHelper.GetDescription((FaceSize)level.nSmall, _useChinese),
                IntValue = level.nSmall
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "姿态" : "Posture",
                Value = EnumHelper.GetDescription((FacePosture)level.nPosture, _useChinese),
                IntValue = level.nPosture
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "脸部遮挡" : "Mask",
                Value = EnumHelper.GetDescription((FaceMaskOcclusion)level.nMask, _useChinese),
                IntValue = level.nMask
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "口罩" : "FaceMask",
                Value = EnumHelper.GetDescription((FaceMask)level.nFaceMask, _useChinese),
                IntValue = level.nFaceMask
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "帽子" : "Hat",
                Value = EnumHelper.GetDescription((FaceHat)level.nHat, _useChinese),
                IntValue = level.nHat
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "眼镜" : "Glasses",
                Value = EnumHelper.GetDescription((FaceGlasses)level.nGlasses, _useChinese),
                IntValue = level.nGlasses
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "张嘴" : "Gape",
                Value = EnumHelper.GetDescription((FaceGape)level.nGape, _useChinese),
                IntValue = level.nGape
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "模糊度" : "Blur",
                Value = EnumHelper.GetDescription((FaceBlur)level.nBlur, _useChinese),
                IntValue = level.nBlur
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "曝光度" : "Bright",
                Value = EnumHelper.GetDescription((FaceBrightness)level.nBright, _useChinese),
                IntValue = level.nBright
            },
            new DisplayFaceQualityLevelItem
            {
                Name = _useChinese ? "光源方向" : "Light",
                Value = EnumHelper.GetDescription((FaceLightDirection)level.nLight, _useChinese),
                IntValue = level.nLight
            }
        };
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Index = i+1;
            }
            return result;
        }
    }
}

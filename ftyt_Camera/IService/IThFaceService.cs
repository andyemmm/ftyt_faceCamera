using ftyt.shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ftyt.ftyt_Camera.Models.FaceParameterModel;
using static ftyt.ftyt_Camera.Service.ThFaceService;

namespace ftyt.ftyt_Camera.IService
{
    public interface IThFaceService
    {
        Task<AsyncActionResult> IdFaceInit();
        void IdFaceUninit();
        int IdFaceDetectFace(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face);
        int IdFaceFaceQualityLevel(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face, ref FACE_QUALITY_LEVEL FaceQualityLevel);
        int IdFaceFeatureSize();
        int IdFaceFeatureGet(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face, [Out] IntPtr pFeature);

        Byte IdFaceFeatureCompare(IntPtr pFeature1, IntPtr pFeature2);
        Int32 IdFaceFaceFeature(IntPtr pImage, Int32 nWidth, Int32 nHeight, Int32 nMaxFace, IntPtr Faces, [Out] IntPtr pFeatures);
        Int32 IdFaceFaceFeature(IntPtr pImage, Int32 nWidth, Int32 nHeight, Int32 nMaxFace, [Out] FACE_DETECT_RESULT[] Faces, [Out] IntPtr pFeatures);
        Int32 IdFaceGetLiveFaceStatus();
        Int32 IdFaceLiveFaceDetectEx(Int32 nWidth, Int32 nHeight, IntPtr pImageColor, ref FACE_DETECT_RESULT FaceColor, IntPtr pImageBW, ref FACE_DETECT_RESULT FaceBW, ref Int32 nScore);

        IntPtr IdFaceListCreate(Int32 nMaxFeatureNum);

        Int32 IdFaceListInsert(IntPtr hList, [In, Out] ref Int32 nPos, IntPtr pFeatures, Int32 nFeatureNum);

        Int32 IdFaceListRemove(IntPtr hList, Int32 nPos, Int32 nFeatureNum);

        void IdFaceListClearAll(IntPtr hList);

        Int32 IdFaceListCompare(IntPtr hList, IntPtr pFeature, Int32 nPosBegin, Int32 nFeatureNum, [Out] IntPtr pnScores);

        void IdFaceListDestroy(IntPtr hList);

        Int32 IdFaceReadImageFile(IntPtr filename, [Out] IntPtr pRgbBuf, Int32 nBufSize, ref Int32 nWidth, ref Int32 nHeight, Int32 nDepth);
        Int32 IdFaceReadImageFileData(IntPtr pFileData, Int32 nFileDataSize, [Out] IntPtr pRgbBuf, Int32 nBufSize, ref Int32 nWidth, ref Int32 nHeight, Int32 nDepth);
        Int32 IdFaceRotateRgb24Data(IntPtr pSrc, Int32 nWidth, Int32 nHeight, Int32 nDegree, Int32 nMirror, [Out] IntPtr pDst);
        Int32 IdFaceSaveJpegFile(IntPtr filename, IntPtr pRgbData, Int32 nWidth, Int32 nHeight, Int32 nDepth, Int32 nQuality);
        Int32 IdFaceSaveJpegFileData(IntPtr pRgbData, Int32 nWidth, Int32 nHeight, Int32 nDepth, Int32 nQuality, [Out] IntPtr pFileDataBuf, Int32 nBufSize, ref Int32 nFileDataSize);
    }
}

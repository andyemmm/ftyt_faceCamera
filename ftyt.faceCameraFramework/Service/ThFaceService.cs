using ftyt.faceCameraFramework.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ftyt.faceCameraFramework.Models.FaceParameterModel;

namespace ftyt.faceCameraFramework.Service
{
    public class ThFaceService : IThFaceService
    {
        readonly LogService _logService;

        public ThFaceService(LogService logService)
        {
            _logService = logService;
        }

        #region 初始化及基本功能

        // 返回SDK版本号（随时可调用）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkVer", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkVer();

        // 返回设备运行码
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkGetRunCode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkGetRunCode([Out] IntPtr pStrRunCode);

        // SDK初始化，成功返回0，许可无效返回-1，算法初始化失败返回-2（后面除辅助接口外的所有功能接口都必须是SDK初始化成功后才有用）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkInit();

        // SDK初始化并设置是否快速检测，成功返回0，许可无效返回-1，算法初始化失败返回-2（后面除辅助接口外的所有功能接口都必须是SDK初始化成功后才有用）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkInitEx", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkInitEx(bool bQuickDetect);

        // SDK初始化并开启调试日志，成功返回0，许可无效返回-1，算法初始化失败返回-2（后面除辅助接口外的所有功能接口都必须是SDK初始化成功后才有用）
        // 备注：IdFaceSdkInit采用快速检测
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkInitEx", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkInitExDebug(bool bQuickDetect);

        // SDK反初始化（后面除辅助接口外的所有功能接口在调用反初始化后均不可用，除非再次初始化）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkUninit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void IdFaceSdkUninit();

        // 设置检测大小（针对高分辨率且人脸占比较小时设置检测大小，通常不必调用）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkSetDetectSize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void IdFaceSdkSetDetectSize(Int32 nDetectSize);

        // 返回特征码大小
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFeatureSize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkFeatureSize();

        // 返回当前的授权是否支持活体检测
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkGetLiveFaceStatus", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkGetLiveFaceStatus();

        #endregion

        #region 单人脸检测

        // 检测最大人脸
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkDetectFace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkDetectFace(IntPtr pImage, Int32 nWidth, Int32 nHeight, ref FACE_DETECT_RESULT Face);

        #endregion

        #region 多人脸检测并提取特征

        // 检测多人脸同时提取各人脸的特征（nMaxFace 表示最多要检测的人脸个数，Faces 必须按最大人脸个数分配人脸坐标空间， pFeatures 必须按最大人脸个数分配特征码空间，pFeatures 参数传 0 时则只检测人脸不提特征）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFaceFeature", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkFaceFeature(IntPtr pImage, Int32 nWidth, Int32 nHeight, Int32 nMaxFace, IntPtr Faces, [Out] IntPtr pFeatures);

        #endregion

        #region 多人脸检测并提取特征

        // 检测多人脸同时提取各人脸的特征（nMaxFace 表示最多要检测的人脸个数，Faces 必须按最大人脸个数分配人脸坐标空间， pFeatures 必须按最大人脸个数分配特征码空间，pFeatures 参数传 0 时则只检测人脸不提特征）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFaceFeature", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkFaceFeature(IntPtr pImage, Int32 nWidth, Int32 nHeight, Int32 nMaxFace, [Out] FACE_DETECT_RESULT[] Faces, [Out] IntPtr pFeatures);

        #endregion

        #region 人脸质量检测

        // 检测人脸质量（需输入人脸检测返回的人脸坐标）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFaceQualityLevel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkFaceQualityLevel(IntPtr pImage, Int32 nWidth, Int32 nHeight, ref FACE_DETECT_RESULT Face, ref FACE_QUALITY_LEVEL FaceQualityLevel);

        #endregion

        #region SDK特征提取

        // 提取人脸特征（需输入人脸检测返回的人脸坐标，pFeature需分配不小于一个人脸特征的空间）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFeatureGet", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkFeatureGet(IntPtr pImage, Int32 nWidth, Int32 nHeight, ref FACE_DETECT_RESULT Face, [Out] IntPtr pFeature);

        #endregion

        #region 一对一比对（1:1，多用于人证核验）

        // 两个人脸特征比对出相似度
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkFeatureCompare", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Byte IdFaceSdkFeatureCompare(IntPtr pFeature1, IntPtr pFeature2);

        #endregion

        #region 一对多比对（1:N，多用于服务器识别）

        // 创建一对多人脸比对列表
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListCreate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr IdFaceSdkListCreate(Int32 nMaxFeatureNum);

        // 向人脸比对列表中增加/插入模板的人脸特征
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListInsert", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkListInsert(IntPtr hList, [In, Out] ref Int32 nPos, IntPtr pFeatures, Int32 nFeatureNum);

        // 从人脸比对列表中删除部分人脸特征
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListRemove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkListRemove(IntPtr hList, Int32 nPos, Int32 nFeatureNum);

        // 清空人脸比对列表中的所有人脸特征
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListClearAll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void IdFaceSdkListClearAll(IntPtr hList);

        // 一对多人脸比对，返回参与比对的特征数，pnScores 需分配不小于模板特征数的空间，调用后将输出与每个模板特征比对的结果（特征相似度）
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListCompare", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkListCompare(IntPtr hList, IntPtr pFeature, Int32 nPosBegin, Int32 nFeatureNum, [Out] IntPtr pnScores);

        // 销毁一对多特征比对列表
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkListDestroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void IdFaceSdkListDestroy(IntPtr hList);

        #endregion

        #region 活体检测

        // 活体检测（返回1表示活体），需传入人脸检测返回的人脸坐标，pImageColor 与 pImageBW 均有效则进行双目活体检测，如 pImageBW 为 0 则进行彩色单目活体检测，pImageColor 为 0 则进行红外双目活体检测
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkLiveFaceDetect", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkLiveFaceDetect(Int32 nWidth, Int32 nHeight, IntPtr pImageColor, ref FACE_DETECT_RESULT FaceColor, IntPtr pImageBW, ref FACE_DETECT_RESULT FaceBW);

        // 活体检测并输出活检分数（可根据活检分数是否达到阈值判别是否为活体），需传入人脸检测返回的人脸坐标，pImageColor 与 pImageBW 均有效则进行双目活体检测，如 pImageBW 为 0 则进行彩色单目活体检测，pImageColor 为 0 则进行红外双目活体检测
        [DllImport("IdFaceSdk.dll", EntryPoint = "IdFaceSdkLiveFaceDetectEx", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 IdFaceSdkLiveFaceDetectEx(Int32 nWidth, Int32 nHeight, IntPtr pImageColor, ref FACE_DETECT_RESULT FaceColor, IntPtr pImageBW, ref FACE_DETECT_RESULT FaceBW, ref Int32 nScore);

        #endregion

        #region 辅助接口

        // 读图象文件到RGB24图象数据缓冲区，支持BMP、JPG、PNG图象文件，pRgbBuf 必须分配足够的缓冲区（不小于 nWidth * nHeight * 3）,如不知道图象分辨率可将此参数传 0 则本次调用只返回图象分辨率，然后分配足够的缓冲区再次调用读出图象数据
        [DllImport("IdFaceSdk.dll", EntryPoint = "ReadImageFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadImageFile(IntPtr filename, [Out] IntPtr pRgbBuf, Int32 nBufSize, ref Int32 nWidth, ref Int32 nHeight, Int32 nDepth);

        // 读图象文件数据到RGB图象数据缓冲区，支持BMP、JPG、PNG图象文件，pRgbBuf 必须分配足够的缓冲区（不小于 nWidth * nHeight * 3）,如不知道图象分辨率可将此参数传 0 则本次调用只返回图象分辨率，然后分配足够的缓冲区再次调用读出图象数据
        [DllImport("IdFaceSdk.dll", EntryPoint = "ReadImageFileData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadImageFileData(IntPtr pFileData, Int32 nFileDataSize, [Out] IntPtr pRgbBuf, Int32 nBufSize, ref Int32 nWidth, ref Int32 nHeight, Int32 nDepth);

        // 旋转RGB24图象数据，nDegree为旋转角度（支持0、90、180、270），nMirror为0表示不镜象，为1表示左右镜象
        [DllImport("IdFaceSdk.dll", EntryPoint = "RotateRgb24Data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 RotateRgb24Data(IntPtr pSrc, Int32 nWidth, Int32 nHeight, Int32 nDegree, Int32 nMirror, [Out] IntPtr pDst);

        // 将RGB24图象数据保存为JPEG文件
        [DllImport("IdFaceSdk.dll", EntryPoint = "SaveJpegFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SaveJpegFile(IntPtr filename, IntPtr pRgbData, Int32 nWidth, Int32 nHeight, Int32 nDepth, Int32 nQuality);

        // 将RGB24图象数据保存为JPEG文件数据
        [DllImport("IdFaceSdk.dll", EntryPoint = "SaveJpegFileData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SaveJpegFileData(IntPtr pRgbData, Int32 nWidth, Int32 nHeight, Int32 nDepth, Int32 nQuality, [Out] IntPtr pFileDataBuf, Int32 nBufSize, ref Int32 nFileDataSize);

        #endregion


        //初始化
        public Task<AsyncActionResult> IdFaceInit()
        {
            try
            {
                int initResult = IdFaceSdkInit();
                if (initResult == 0)
                {
                    _logService.Info($"[IdFace]Init Succcess");
                    return Task.FromResult(AsyncActionResult.Succcess);
                }
                else if (initResult == -1)
                {
                    _logService.Info($"[IdFace]Init Unauthorized");
                    return Task.FromResult(new AsyncActionResult("Unauthorized"));
                }
                else
                {
                    _logService.Info($"[IdFace]Initialization failed");
                    return Task.FromResult(new AsyncActionResult("Initialization failed"));
                }
            }
            catch (Exception ex)
            {
                _logService.Info($"[IdFace]failed:{ex.Message}");
                return Task.FromResult(new AsyncActionResult(ex.Message));
            }
        }

        //反初始化
        public void IdFaceUninit()
        {
            IdFaceSdkUninit();
        }

        //检测人脸
        public int IdFaceDetectFace(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face)
        {
            return IdFaceSdkDetectFace(pImage, nWidth, nHeight, ref Face);
        }

        // 检测人脸质量（需输入人脸检测返回的人脸坐标）
        public int IdFaceFaceQualityLevel(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face, ref FACE_QUALITY_LEVEL FaceQualityLevel)
        {
            return IdFaceSdkFaceQualityLevel(pImage, nWidth, nHeight, ref Face, ref FaceQualityLevel);
        }

        // 返回特征码大小
        public int IdFaceFeatureSize()
        {
            return IdFaceSdkFeatureSize();
        }

        // 提取人脸特征（需输入人脸检测返回的人脸坐标，pFeature需分配不小于一个人脸特征的空间）
        public int IdFaceFeatureGet(IntPtr pImage, int nWidth, int nHeight, ref FACE_DETECT_RESULT Face, [Out] IntPtr pFeature)
        {
            return IdFaceSdkFeatureGet(pImage, nWidth, nHeight, ref Face, pFeature);
        }

        // 两个人脸特征比对出相似度
        public byte IdFaceFeatureCompare(IntPtr pFeature1, IntPtr pFeature2)
        {
            return IdFaceSdkFeatureCompare(pFeature1, pFeature2);
        }

        // 检测多人脸同时提取各人脸的特征（nMaxFace 表示最多要检测的人脸个数，Faces 必须按最大人脸个数分配人脸坐标空间， pFeatures 必须按最大人脸个数分配特征码空间，pFeatures 参数传 0 时则只检测人脸不提特征）
        public int IdFaceFaceFeature(IntPtr pImage, int nWidth, int nHeight, int nMaxFace, [Out] FACE_DETECT_RESULT[] Faces, [Out] IntPtr pFeatures)
        {
            return IdFaceSdkFaceFeature(pImage, nWidth, nHeight, nMaxFace, Faces, pFeatures);
        }        
        
        // 检测多人脸同时提取各人脸的特征（nMaxFace 表示最多要检测的人脸个数，Faces 必须按最大人脸个数分配人脸坐标空间， pFeatures 必须按最大人脸个数分配特征码空间，pFeatures 参数传 0 时则只检测人脸不提特征）
        public int IdFaceFaceFeature(IntPtr pImage, int nWidth, int nHeight, int nMaxFace, IntPtr Faces, [Out] IntPtr pFeatures)
        {
            return IdFaceSdkFaceFeature(pImage, nWidth, nHeight, nMaxFace, Faces, pFeatures);
        }

        // 返回当前的授权是否支持活体检测
        public int IdFaceGetLiveFaceStatus()
        {
            return IdFaceSdkGetLiveFaceStatus();
        }

        // 活体检测并输出活检分数（可根据活检分数是否达到阈值判别是否为活体），需传入人脸检测返回的人脸坐标，pImageColor 与 pImageBW 均有效则进行双目活体检测，如 pImageBW 为 0 则进行彩色单目活体检测，pImageColor 为 0 则进行红外双目活体检测
        public int IdFaceLiveFaceDetectEx(int nWidth, int nHeight, IntPtr pImageColor, ref FACE_DETECT_RESULT FaceColor, IntPtr pImageBW, ref FACE_DETECT_RESULT FaceBW, ref int nScore)
        {
            return IdFaceSdkLiveFaceDetectEx(nWidth, nHeight, pImageColor, ref FaceColor, pImageBW, ref FaceBW, ref nScore);
        }

        // 创建一对多人脸比对列表
        public IntPtr IdFaceListCreate(int nMaxFeatureNum)
        {
            return IdFaceSdkListCreate(nMaxFeatureNum);
        }

        // 向人脸比对列表中增加/插入模板的人脸特征
        public int IdFaceListInsert(IntPtr hList, [In, Out] ref int nPos, IntPtr pFeatures, int nFeatureNum)
        {
            return IdFaceSdkListInsert(hList, ref nPos, pFeatures, nFeatureNum);
        }

        // 从人脸比对列表中删除部分人脸特征
        public int IdFaceListRemove(IntPtr hList, int nPos, int nFeatureNum)
        {
            return IdFaceSdkListRemove(hList, nPos, nFeatureNum);
        }

        // 清空人脸比对列表中的所有人脸特征
        public void IdFaceListClearAll(IntPtr hList)
        {
            IdFaceSdkListClearAll(hList);
        }

        // 一对多人脸比对，返回参与比对的特征数，pnScores 需分配不小于模板特征数的空间，调用后将输出与每个模板特征比对的结果（特征相似度）
        public int IdFaceListCompare(IntPtr hList, IntPtr pFeature, int nPosBegin, int nFeatureNum, [Out] IntPtr pnScores)
        {
            return IdFaceSdkListCompare(hList, pFeature, nPosBegin, nFeatureNum, pnScores);
        }

        // 销毁一对多特征比对列表
        public void IdFaceListDestroy(IntPtr hList)
        {
            IdFaceSdkListDestroy(hList);
        }

        public int IdFaceReadImageFile(IntPtr filename, [Out] IntPtr pRgbBuf, int nBufSize, ref int nWidth, ref int nHeight, int nDepth)
        {
            return ReadImageFile(filename, pRgbBuf, nBufSize, ref nWidth, ref nHeight, nDepth);
        }

        public int IdFaceReadImageFileData(IntPtr pFileData, int nFileDataSize, [Out] IntPtr pRgbBuf, int nBufSize, ref int nWidth, ref int nHeight, int nDepth)
        {
            return ReadImageFileData(pFileData, nFileDataSize, pRgbBuf, nBufSize, ref nWidth, ref nHeight, nDepth);
        }

        public int IdFaceRotateRgb24Data(IntPtr pSrc, int nWidth, int nHeight, int nDegree, int nMirror, [Out] IntPtr pDst)
        {
            return RotateRgb24Data(pSrc, nWidth, nHeight, nDegree, nMirror, pDst);
        }

        public int IdFaceSaveJpegFile(IntPtr filename, IntPtr pRgbData, int nWidth, int nHeight, int nDepth, int nQuality)
        {
            return SaveJpegFile(filename, pRgbData, nWidth, nHeight, nDepth, nQuality);
        }

        public int IdFaceSaveJpegFileData(IntPtr pRgbData, int nWidth, int nHeight, int nDepth, int nQuality, [Out] IntPtr pFileDataBuf, int nBufSize, ref int nFileDataSize)
        {
            return SaveJpegFileData(pRgbData, nWidth, nHeight, nDepth, nQuality, pFileDataBuf, nBufSize, ref nFileDataSize);
        }
    }
}

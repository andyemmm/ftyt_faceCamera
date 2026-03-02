using ftyt.faceCameraFramework.IService;
using ftyt.faceCameraFramework.Models;
using ftyt.faceCameraFramework.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using IDialogService = ftyt.faceCameraFramework.IService.IDialogService;
using System.Runtime.InteropServices;
using static ftyt.faceCameraFramework.Models.FaceParameterModel;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using log4net.Core;
using static ftyt.faceCameraFramework.Enums.FaceQualityLevel;
using ftyt.faceCameraFramework.Extensions;
using ftyt.faceCameraFramework.Converters;
using ftyt.faceCameraFramework.Enums;
using Prism.Mvvm;
using System.Threading;
using Prism.Events;
using Prism.Commands;
using System.Windows.Shapes;
using Prism.Regions;
using System.Windows.Media.Media3D;
using System.IO;

using DirectShowLib;
using System.Diagnostics.Eventing.Reader;

namespace ftyt.faceCameraFramework.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        readonly DemoAppSettingService _appSettingService;
        readonly IRegionManager _regionManager;
        readonly IDialogService _dialogService;
        readonly IThFaceService _ThFaceService;
        readonly ICaptureService _captureService;
        readonly LogService _logService;
        private bool _startCapture = true;
        private bool isPreview = false;
        private int RGBNumber = 0, IRNumber = 0;

        //控制采集任务
        private Task _captureTask;
        //控制人脸检测任务
        private Task _detectTask;

        //控制预览任务的启停
        private CancellationTokenSource _captureTokenSource;
        //控制检测任务的启停
        private CancellationTokenSource _detectTokenSource;

        DateTime _mouseDownDateTime;

        Boolean bSdkInit = false, bCameraReady = false, bFramesReady = false, isDetectFace = false;
        Int32 nWidth = 0, nHeight = 0, nFrameShow = 0, nFrameDetect = 0;
        float fExtendLeft = 0.15f, fExtendRight = 0.15f, fExtendTop = 0.15f, fExtendBottom = 0.20f; // 界面显示及保存图象的人脸大小相对原始SDK检测出的人脸大小的放大倍数，为0表示不放大
        IntPtr hCamColor, hCamGray; // 相机组件对象

        IntPtr pFrameColor, pFrameGray; // 保存彩色相机和红外相机的图象帧数据
        IntPtr pFrameTemp1, pFrameTemp2; // 临时图象帧数据缓冲区
        byte[] bTempBuffer; // 临时数据缓冲区
        byte[] CheckResultTempBuffer;
        BitmapSource imageData = null;

        FACE_DETECT_RESULT FaceColor, FaceColorExt; // SDK检测到的彩色图象原始人脸坐标，以及界面显示及保存图象的人脸坐标（在原始坐标基础上根据fExtend?变量放大）
        FACE_DETECT_RESULT FaceGray, FaceGrayExt; // SDK检测到的红外图象原始人脸坐标，以及界面显示及保存图象的人脸坐标（在原始坐标基础上根据fExtend?变量放大）




        public ShellViewModel(IEventAggregator eventAggregator, DemoAppSettingService appSettingService, IRegionManager regionManager, IDialogService dialogService, IThFaceService thFaceService, LogService logService, ICaptureService captureService)
        {
            _appSettingService = appSettingService;
            _regionManager = regionManager;
            _dialogService = dialogService;


            Task.Run(() => UpdateDateTimeNow());

            _logService = logService;
            _captureService = captureService;
            _ThFaceService = thFaceService;
            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < systemCameras.Length; i++)
            {
                if (systemCameras[i].Name == "IR Camera")
                {
                    IRNumber = i;
                }
                if (systemCameras[i].Name == "RGB Camera")
                {
                    RGBNumber = i;
                }
            }


            InitializeCommand = new DelegateCommand(HandleInitializeAsync);
            TapCardCommand = new DelegateCommand(TapCardClick);

        }



        public ShellModel Model { get; } = new ShellModel();
        public ICommand InitializeCommand { get; }
        public ICommand CaptureFrameCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand DetectFaceCommand { get; }
        public ICommand LiveFaceCommand { get; }
        public ICommand DetectFaceQualityCommand { get; }
        public ICommand TapCardCommand { get; }
        public ICommand CleanFaceCommand { get; }



        private Visibility _showMessageBlockVisibility = Visibility.Collapsed;
        public Visibility ShowMessageBlockVisibility
        {
            get => _showMessageBlockVisibility;
            set
            {
                _showMessageBlockVisibility = value;
                OnShowMessagePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler ShowMessagePropertyChanged;
        protected void OnShowMessagePropertyChanged([CallerMemberName] string propName = null)
        {
            ShowMessagePropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        readonly object _captureLock = new object();

        void HandleInitializeAsync()
        {
            Model.PreviewButtonName = "Preview";
            Model.LiveFaceButtonName = "StartLiveDetect";
            Preview();
            KeepDetectFace();
        }

        bool shouldPause = false;
        async void Preview()
        {
            try
            {
                if (isPreview == false)
                {
                    var actionResult = await _ThFaceService.IdFaceInit();
                    if (!actionResult.IsSuccess)
                    {
                        ShowMessageBlockVisibility = Visibility.Visible;
                        Model.Message = actionResult.Error;
                    }
                    else
                    {
                        // 分配彩色帧缓冲区、红外帧缓冲区及临时帧数据缓冲区
                        pFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
                        pFrameGray = Marshal.AllocHGlobal(1920 * 1080 * 3);
                        pFrameTemp1 = Marshal.AllocHGlobal(1920 * 1080 * 3);
                        pFrameTemp2 = Marshal.AllocHGlobal(1920 * 1080 * 3);
                        bTempBuffer = new byte[1920 * 1080 * 3];
                        CheckResultTempBuffer = new byte[1920 * 1080 * 3]; // 临时数据缓冲区

                        FaceGray = new FACE_DETECT_RESULT();
                        FaceGrayExt = new FACE_DETECT_RESULT();

                        bSdkInit = true;
                        // 打开彩色相机
                        hCamColor = _captureService.CAPTURE_INIT();
                        _captureService.CAPTURE_OPEN(hCamColor, RGBNumber);


                        // 打开红外相机
                        hCamGray = _captureService.CAPTURE_INIT();
                        _captureService.CAPTURE_OPEN(hCamGray, IRNumber);

                        if (_captureTokenSource != null)
                        {
                            _captureTokenSource.Dispose();
                        }
                        _captureTokenSource = new CancellationTokenSource();
                        var token = _captureTokenSource.Token;

                        // 启动一个循环采集线程
                        _captureTask = Task.Run(async () =>
                        {
                            while (!token.IsCancellationRequested)
                            {
                                if (shouldPause)
                                {
                                    Model.Status = "Success";
                                    try
                                    {
                                        PreviewImage = null;
                                        await Task.Delay(_appSettingService.Content.Camera.CaptureSuccessDuration, token); // 暂停5秒
                                    }
                                    catch (TaskCanceledException)
                                    {
                                        break;
                                    }
                                    CaptureImage = null;
                                    Model.FaceQualityLevelItem = null;
                                    Model.Message = ""; 
                                    Model.Status = "Preview";
                                    shouldPause = false;
                                    continue; // 跳过本轮，进入下一轮循环
                                }
                                else
                                {
                                    lock (_captureLock)
                                    {
                                        // 从摄像头获取图像数据
                                        imageData = CaptureAndShowImage();
                                    }
                                    try
                                    {
                                        //var image = CaptureAndShowImage();
                                        if (Application.Current == null)
                                        {
                                            return;
                                        }
                                        await Application.Current.Dispatcher.InvokeAsync(() =>
                                        {
                                            if (isPreview) // 避免UI线程调用后又清空导致错误
                                                PreviewImage = imageData;
                                        });
                                        try
                                        {
                                            await Task.Delay(20, token); // 控制帧率
                                        }
                                        catch (TaskCanceledException)
                                        {
                                            break; // 安全退出循环
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logService.Error("图像刷新异常", ex);
                                    }
                                }
                            }
                        }, token);

                        isPreview = true;
                        Model.PreviewButtonName = "StopPreview";
                    }
                }
                else
                {
                    isPreview = false;
                    _captureTokenSource?.Cancel();
                    if (_captureTask != null)
                    {
                        try
                        {
                            await _captureTask;
                        }
                        catch (TaskCanceledException) { }
                        _captureTask = null;
                    }
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        PreviewImage = null;
                    });
                    await StopAsync();
                    Model.PreviewButtonName = "Preview";
                }
            }

            catch (Exception ex)
            {
                _logService.Info($"[Preview] {ex}");
            }
        }


        void UpdateDateTimeNow()
        {
            while (true)
            {
                Model.DateTime = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                Thread.Sleep(1000);
            }
        }


        private BitmapSource _previewImage;
        public BitmapSource PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        private BitmapSource _captureImage;
        public BitmapSource CaptureImage
        {
            get => _captureImage;
            set => SetProperty(ref _captureImage, value);
        }

        private BitmapSource CaptureAndShowImage()
        {
            int nLen = 0, nWidth2 = 0, nHeight2 = 0;
            // 获取图像帧数据
            int result = _captureService.CAPTURE_GETFRAME(hCamColor, pFrameTemp1, ref nLen, ref nWidth2, ref nHeight2);

            if (result == 0 && nLen > 0)
            {
                bFramesReady = true;
                // 将数据复制到 byte[] 缓冲
                Marshal.Copy(pFrameTemp1, bTempBuffer, 0, nLen);
                // 调试：检查前 100 字节
                for (int i = 0; i < Math.Min(100, nLen); i++)
                {
                    Debug.Write($"{bTempBuffer[i]:X2} ");
                    if ((i + 1) % 16 == 0) Debug.WriteLine("");
                }
                // 返回一帧图像
                var bitmap = CreateBitmapFromBuffer(bTempBuffer, nWidth2, nHeight2);
                return bitmap;
            }
            else
            {
                return null;
            }
        }


        int nLen = 0, nWidth2 = 0, nHeight2 = 0;

        private BitmapSource GetImage(IntPtr CheckResultFrameTemp, int Len, int Width, int Height)
        {
            //int result = _captureService.CAPTURE_GETFRAME(hCamColor, CheckResultFrameTemp, ref nLen, ref Width, ref Height);
            _logService.Info($"GetImage(IntPtr) , Length: {Len}, Width: {Width}, Height: {Height}");
            // 将数据复制到 byte[] 缓冲
            Marshal.Copy(CheckResultFrameTemp, CheckResultTempBuffer, 0, Len);
            // 调试：检查前 100 字节
            for (int i = 0; i < Math.Min(100, Len); i++)
            {
                Debug.Write($"{CheckResultTempBuffer[i]:X2} ");
                if ((i + 1) % 16 == 0) Debug.WriteLine("");
            }
            // 返回一帧图像
            var bitmap = CreateBitmapFromBuffer(CheckResultTempBuffer, Width, Height);
            return bitmap;
        }

        private BitmapSource GetImage(IntPtr CheckResultFrameTemp, int Len, int Width, int Height, int left, int top, int right, int bottom)
        {
            _logService.Info($"GetImage(IntPtr) , Length: {Len}, Width: {Width}, Height: {Height}");

            Marshal.Copy(CheckResultFrameTemp, CheckResultTempBuffer, 0, Len);

            for (int i = 0; i < Math.Min(100, Len); i++)
            {
                Debug.Write($"{CheckResultTempBuffer[i]:X2} ");
                if ((i + 1) % 16 == 0) Debug.WriteLine("");
            }

            var bitmap = CreateBitmapFromBuffer(CheckResultTempBuffer, Width, Height, left, top, right, bottom);
            return bitmap;
        }

        public void SaveFrameToFile(byte[] buffer, int width, int height, string filePath)
        {
            using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                var bmpData = bmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                int stride = bmpData.Stride;
                IntPtr ptr = bmpData.Scan0;

                // 注意：每行可能有 padding，所以不能直接复制整个 buffer
                for (int y = 0; y < height; y++)
                {
                    IntPtr rowPtr = ptr + y * stride;
                    int offset = y * width * 3;
                    Marshal.Copy(buffer, offset, rowPtr, width * 3);
                }

                bmp.UnlockBits(bmpData);
                bmp.Save(filePath, ImageFormat.Bmp);
            }
        }
        public BitmapSource CreateBitmapFromBuffer(byte[] buffer, int width, int height)
        {
            int stride = width * 3;

            var bitmap = BitmapSource.Create(
                width,
                height,
                96, 96,
                PixelFormats.Bgr24,
                null,
                buffer,
                stride);

            bitmap.Freeze();
            return bitmap;
        }

        public BitmapSource CreateBitmapFromBuffer(byte[] buffer, int sourceWidth, int sourceHeight, int left, int top, int right, int bottom)
        {
            // 校正坐标防止越界
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (right > sourceWidth) right = sourceWidth;
            if (bottom > sourceHeight) bottom = sourceHeight;

            int cropWidth = right - left;
            int cropHeight = bottom - top;
            int sourceStride = sourceWidth * 3;
            int croppedStride = cropWidth * 3;

            byte[] croppedBuffer = new byte[cropHeight * croppedStride];

            for (int y = 0; y < cropHeight; y++)
            {
                int sourceIndex = (top + y) * sourceStride + (left * 3);
                int targetIndex = y * croppedStride;
                Array.Copy(buffer, sourceIndex, croppedBuffer, targetIndex, croppedStride);
            }

            var croppedBitmap = BitmapSource.Create(
                cropWidth,
                cropHeight,
                96, 96,
                PixelFormats.Bgr24,
                null,
                croppedBuffer,
                croppedStride);
            //var flipTransform = new ScaleTransform(-1, 1, cropWidth / 2.0, cropHeight / 2.0);
            //var flippedBitmap = new TransformedBitmap(croppedBitmap, flipTransform);
            croppedBitmap.Freeze();
            return croppedBitmap;
        }

        readonly object _detectLock = new object();


        void KeepDetectFace()
        {
            try
            {
                if (_detectTokenSource != null)
                {
                    _detectTokenSource.Dispose();
                }
                _detectTokenSource = new CancellationTokenSource();
                var detecttoken = _detectTokenSource.Token;
                // 启动一个循环采集线程
                _detectTask = Task.Run(async () =>
                {
                    while (!detecttoken.IsCancellationRequested)
                    {
                        if (isDetectFace)
                        {
                            try
                            {
                                lock (_detectLock)
                                {
                                    TapCardTakePhoto();
                                    //TestFaceFeatureCount();
                                }
                                await Task.Delay(_appSettingService.Content.Camera.DetectInterval, detecttoken); // 控制帧率
                            }
                            catch (Exception ex)
                            {
                                _logService.Error("人脸检测异常", ex);
                                break; // 安全退出循环
                            }
                        }
                    }
                }, detecttoken);
            }
            catch (Exception ex)
            {
                _logService.Info($"[StartDetectFace] {ex}");
            }
        }




        IntPtr DetectFrameColor; // 保存彩色相机和红外相机的图象帧数据用于人脸检测
        IntPtr DetectFrameTemp1; // 临时图象帧数据缓冲区用于人脸检测



        private void TapCardClick()
        {
            aaa = 0;
            isDetectFace = true;
            faceInfo = null;
            Model.Status = "Capturing";
        }

        IntPtr TapCardFrameColor; // 保存彩色相机和红外相机的图象帧数据用于人脸检测
        IntPtr TapCardFrameTemp; // 临时图象帧数据缓冲区用于人脸检测


        IntPtr TapCardFrameGray; // 保存红外相机和红外相机的图象帧数据用于人脸检测
        IntPtr TapCardFrameGrayTemp; // 红外临时图象帧数据缓冲区用于人脸检测

        int TapCardLen = 0, TapCardWidth = 0, TapCardHeight = 0;
        int TapCardGrayLen = 0, TapCardGrayWidth = 0, TapCardGrayHeight = 0;
        StringBuilder messageInfo = new StringBuilder();
        FaceInfo faceInfo = null;
        int maxFaces = 2;
        int featureSize = 512;
        IntPtr featuresPtr;
        int aaaa = 0;

        //刷卡后进行人脸拍照返回照片与检测质量
        private async void TapCardTakePhoto()
        {
            List<DisplayFaceQualityLevelItem> displayList = null;
            if (messageInfo != null && messageInfo.Length > 0) messageInfo.Clear();
            if (TapCardFrameColor != IntPtr.Zero) Marshal.FreeHGlobal(TapCardFrameColor);
            if (TapCardFrameTemp != IntPtr.Zero) Marshal.FreeHGlobal(TapCardFrameTemp);

            if (TapCardFrameGray != IntPtr.Zero) Marshal.FreeHGlobal(TapCardFrameGray);
            if (TapCardFrameGrayTemp != IntPtr.Zero) Marshal.FreeHGlobal(TapCardFrameGrayTemp);
            if (faceInfo == null)
            {
                faceInfo = new FaceInfo();
            }

            TapCardFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
            TapCardFrameTemp = Marshal.AllocHGlobal(1920 * 1080 * 3);

            TapCardFrameGray = Marshal.AllocHGlobal(1920 * 1080 * 3);
            TapCardFrameGrayTemp = Marshal.AllocHGlobal(1920 * 1080 * 3);

            // 获取图像帧数据
            int result = _captureService.CAPTURE_GETFRAME(hCamColor, TapCardFrameTemp, ref TapCardLen, ref TapCardWidth, ref TapCardHeight);
            // 红外获取图像帧数据
            int resultGray = _captureService.CAPTURE_GETFRAME(hCamGray, TapCardFrameGrayTemp, ref TapCardGrayLen, ref TapCardGrayWidth, ref TapCardGrayHeight);
            _logService.Info($"GETFRAME result: {result}, Length: {TapCardLen}, Width: {TapCardWidth}, Height: {TapCardHeight}");
            _logService.Info($"GETFRAME resultGray: {resultGray}, Length: {TapCardGrayLen}, Width: {TapCardGrayWidth}, Height: {TapCardGrayHeight}");

            if (TapCardWidth > 0 && TapCardHeight > 0)
            {
                // 将彩色图象数据左右镜象后放入 TapCardFrameColor 缓冲区
                _ThFaceService.IdFaceRotateRgb24Data(TapCardFrameTemp, TapCardWidth, TapCardHeight, 0, 1, TapCardFrameColor);
            }
            // 分配彩色原始人脸坐标空间、彩色放大人脸坐标空间、红外原始人脸坐标空间、红外放大人脸坐标空间
            FACE_DETECT_RESULT DetectFaceColor = new FACE_DETECT_RESULT();
            FACE_DETECT_RESULT DetectFaceColorExt = new FACE_DETECT_RESULT();

            if (TapCardGrayWidth > 0 && TapCardGrayHeight > 0)
            {
                // 将红外图象数据左右镜象后放入 TapCardFrameGray 缓冲区
                _ThFaceService.IdFaceRotateRgb24Data(TapCardFrameGrayTemp, TapCardGrayWidth, TapCardGrayHeight, 0, 1, TapCardFrameGray);
            }
            // 分配彩色原始人脸坐标空间、彩色放大人脸坐标空间、红外原始人脸坐标空间、红外放大人脸坐标空间
            FACE_DETECT_RESULT DetectFaceGray = new FACE_DETECT_RESULT();
            FACE_DETECT_RESULT DetectFaceGrayExt = new FACE_DETECT_RESULT();

            //***********
            int maxFaceCount = 2;

            // 分配 FACE_DETECT_RESULT 数组
            int faceStructSize = Marshal.SizeOf(typeof(FACE_DETECT_RESULT));
            int faceStructGraySize = Marshal.SizeOf(typeof(FACE_DETECT_RESULT));

            // 分配非托管内存
            IntPtr pFaceResults = Marshal.AllocHGlobal(faceStructSize * maxFaceCount);
            IntPtr pFaceGrayResults = Marshal.AllocHGlobal(faceStructGraySize * maxFaceCount);

            int colorRet = 0;
            int grayRet = 0;
            try
            {
                // 调用函数
                colorRet = _ThFaceService.IdFaceFaceFeature(TapCardFrameTemp, TapCardWidth, TapCardHeight, maxFaceCount, pFaceResults, IntPtr.Zero);
                grayRet = _ThFaceService.IdFaceFaceFeature(TapCardFrameGrayTemp, TapCardGrayWidth, TapCardGrayHeight, maxFaceCount, pFaceGrayResults, IntPtr.Zero);
                _logService.Info($"人脸数量检测完成，彩色：{colorRet}，红外：{grayRet}");
            }
            catch (Exception ex)
            {
                _logService.Info("人脸数量检测异常：" + ex);
            }

            //彩色或红外检测到人脸大于1
            if (colorRet > 1 || grayRet > 1)
            {
                _logService.Info("多人脸出现在画面中！");
                faceInfo.Message = "多人脸出现在画面中！";
                goto end;
            }
            //***********

            // 对彩色帧图象分别检测人脸
            int nNumColor = _ThFaceService.IdFaceDetectFace(TapCardFrameColor, TapCardWidth, TapCardHeight, ref DetectFaceColor);

            if (nNumColor < 1)
            {
                // 彩色镜头未检测到人脸，清除彩色人脸坐标
                DetectFaceColor.rcFace.left = DetectFaceColor.rcFace.right = 0;
                DetectFaceColor.rcFace.top = DetectFaceColor.rcFace.bottom = TapCardHeight;
                DetectFaceColorExt = DetectFaceColor;
                _logService.Info("请正视摄像头！");
                faceInfo.Message = "请正视摄像头！";
                goto end;
            }
            else
            {
                // 彩色镜头检测到人脸，更新彩色人脸放大坐标
                int w = DetectFaceColor.rcFace.right - DetectFaceColor.rcFace.left, h = DetectFaceColor.rcFace.bottom - DetectFaceColor.rcFace.top;
                int left = DetectFaceColor.rcFace.left - (int)(w * fExtendLeft), right = DetectFaceColor.rcFace.right + (int)(w * fExtendRight), top = DetectFaceColor.rcFace.top - (int)(h * fExtendTop), bottom = DetectFaceColor.rcFace.bottom + (int)(h * fExtendBottom);
                if (left < 0) left = 0;
                if (right >= TapCardWidth) right = TapCardWidth - 1;
                if (top < 0) top = 0;
                if (bottom >= TapCardHeight) bottom = TapCardHeight - 1;

                DetectFaceColorExt.rcFace.left = left;
                DetectFaceColorExt.rcFace.top = top;
                DetectFaceColorExt.rcFace.right = right;
                DetectFaceColorExt.rcFace.bottom = bottom;

            }


            int nNumGray = 0;
            if (_appSettingService.Content.Camera.IREnable)
            {
                nNumGray = _ThFaceService.IdFaceDetectFace(TapCardFrameGray, TapCardWidth, TapCardHeight, ref DetectFaceGray);
                if (nNumGray < 1)
                {
                    // 红外镜头未检测到人脸，清除红外人脸坐标
                    DetectFaceGray.rcFace.left = DetectFaceGray.rcFace.right = 0;
                    DetectFaceGray.rcFace.top = DetectFaceGray.rcFace.bottom = TapCardHeight;
                    DetectFaceGrayExt = DetectFaceGray;
                    _logService.Info("请正视摄像头！");
                    faceInfo.Message = "请正视摄像头！";
                    goto end;
                }
                else
                {
                    // 红外镜头检测到人脸，更新红外人脸放大坐标
                    int w = DetectFaceGray.rcFace.right - DetectFaceGray.rcFace.left, h = DetectFaceGray.rcFace.bottom - DetectFaceGray.rcFace.top;
                    int left = DetectFaceGray.rcFace.left - (int)(w * fExtendLeft), right = DetectFaceGray.rcFace.right + (int)(w * fExtendRight), top = DetectFaceGray.rcFace.top - (int)(h * fExtendTop), bottom = DetectFaceGray.rcFace.bottom + (int)(h * fExtendBottom);
                    if (left < 0) left = 0;
                    if (right >= TapCardWidth) right = TapCardWidth - 1;
                    if (top < 0) top = 0;
                    if (bottom >= TapCardHeight) bottom = TapCardHeight - 1;

                    DetectFaceGrayExt.rcFace.left = left;
                    DetectFaceGrayExt.rcFace.top = top;
                    DetectFaceGrayExt.rcFace.right = right;
                    DetectFaceGrayExt.rcFace.bottom = bottom;

                }
            }

  

            if ((_appSettingService.Content.Camera.IREnable && nNumGray > 0 && nNumColor > 0) 
                ||!_appSettingService.Content.Camera.IREnable && nNumColor > 0) // 彩色和红外能检测到人脸时，进行活体判别
            {
                int nLiveFace = 0;

                int nScore = 0;
                // 人脸检测
                if (_appSettingService.Content.Camera.IREnable)
                {
                    nLiveFace = _ThFaceService.IdFaceLiveFaceDetectEx(TapCardWidth, TapCardHeight, TapCardFrameColor, ref DetectFaceColor, TapCardFrameGray, ref DetectFaceGray, ref nScore);
                }
                else
                {
                    nLiveFace = _ThFaceService.IdFaceLiveFaceDetectEx(TapCardWidth, TapCardHeight, TapCardFrameColor, ref DetectFaceColor, IntPtr.Zero, ref DetectFaceGray, ref nScore);
                }

                if ((_appSettingService.Content.Camera.LiveEnable && nLiveFace == 1 && nScore > _appSettingService.Content.Camera.LiveScore)
                    || (!_appSettingService.Content.Camera.LiveEnable && nLiveFace == 1))
                {
                    faceInfo.IsLiveFace = true;
                    faceInfo.LiveFaceScore = nScore;
                    faceInfo.Message = "确认为活体，分数 " + (_appSettingService.Content.Camera.LiveEnable ? nScore.ToString() : "跳过活检");
                    //messageInfo.Append("确认为活体，分数 " + nScore.ToString());
                    FACE_QUALITY_LEVEL FaceQualityLevel = new FACE_QUALITY_LEVEL();
                    int fFaceQuality = _ThFaceService.IdFaceFaceQualityLevel(TapCardFrameColor, TapCardWidth, TapCardHeight, ref DetectFaceColor, ref FaceQualityLevel);
                    if (fFaceQuality == 0)
                    {
                        var converter = new FaceQualityEnumConverter(useChinese: _appSettingService.Content.Shell.UseChineseEnumDescriptions);
                        faceInfo.DisplayFaceQualityList = converter.Convert(FaceQualityLevel);
                        bool checkResult = CheckFaceQuality(faceInfo.DisplayFaceQualityList);
                        _logService.Info($"checkResult:{checkResult}");
                        if (checkResult)
                        {
                            faceInfo.FaceQuality = true;

                            faceInfo.FaceImage = GetImage(TapCardFrameColor, TapCardLen, TapCardWidth, TapCardHeight, DetectFaceColorExt.rcFace.left, DetectFaceColorExt.rcFace.top, DetectFaceColorExt.rcFace.right, DetectFaceColorExt.rcFace.bottom);
                            faceInfo.FaceResult = true;
                        }
                        else
                        {
                            messageInfo.Append("人脸质量级别检测失败！");
                            faceInfo.FaceQuality = false;
                        }
                    }
                    else
                    {
                        faceInfo.Message = "人脸质量级别检测失败";
                        messageInfo.Append("Face Quality Level Detect Fial");
                        faceInfo.DisplayFaceQualityList = null;
                    }
                }
                else if (nLiveFace == 0 || nScore <= _appSettingService.Content.Camera.LiveScore)
                {
                    faceInfo.IsLiveFace = false;
                    faceInfo.LiveFaceScore = nScore;
                    faceInfo.DisplayFaceQualityList = null;
                    faceInfo.Message = "未确认为活体，分数 " + nScore.ToString();
                }
                else
                {
                    faceInfo.IsLiveFace = false;
                    faceInfo.LiveFaceScore = nScore;
                    faceInfo.DisplayFaceQualityList = null;
                    faceInfo.Message = "活体检测失败，接口返回 " + nLiveFace.ToString();
                }
            }
            else
            {
                faceInfo.DisplayFaceQualityList = null;
                // 未检测到人脸
                faceInfo.Message = "请正视摄像头！";
            }
        end:
            //同步刷新到 UI 预览
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                aaaa++;
                Model.Message = aaaa + ":" + faceInfo.Message.ToString();
                Model.FaceQualityLevelItem = faceInfo.DisplayFaceQualityList;
                if (faceInfo.FaceResult)
                {
                    shouldPause = true;
                    CaptureImage = faceInfo.FaceImage;
                    // DetectFaceColorExt.FaceData
                    isDetectFace = false;

                    string strTimeFlag = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                    string strFolder = Environment.CurrentDirectory + "\\Record";
                    if (System.IO.Directory.Exists(strFolder) == false)
                        System.IO.Directory.CreateDirectory(strFolder);

                    string strFileNameColor = strFolder + "\\" + strTimeFlag + "_" + aaaa.ToString() + "_1.bmp";
                    IntPtr ptrFileNameColor = Marshal.StringToHGlobalAnsi(strFileNameColor);
                    _ThFaceService.IdFaceRotateRgb24Data(TapCardFrameColor, TapCardWidth, TapCardHeight, 180, 1, TapCardFrameTemp);

                    _captureService.CAPTURE_SAVEBMPFILE(ptrFileNameColor, TapCardFrameTemp, TapCardWidth, TapCardHeight,
                         DetectFaceColorExt.rcFace.left,
                         DetectFaceColorExt.rcFace.top,
                         DetectFaceColorExt.rcFace.right,
                         DetectFaceColorExt.rcFace.bottom);
                }
            });
            if (pFaceResults != IntPtr.Zero)
                Marshal.FreeHGlobal(pFaceResults);
            if (pFaceGrayResults != IntPtr.Zero)
                Marshal.FreeHGlobal(pFaceGrayResults);
            if (TapCardFrameColor != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(TapCardFrameColor);
                TapCardFrameColor = IntPtr.Zero;
            }
            if (TapCardFrameTemp != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(TapCardFrameTemp);
                TapCardFrameTemp = IntPtr.Zero;
            }
            if (TapCardFrameGray != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(TapCardFrameGray);
                TapCardFrameGray = IntPtr.Zero;
            }
            if (TapCardFrameGrayTemp != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(TapCardFrameGrayTemp);
                TapCardFrameGrayTemp = IntPtr.Zero;
            }
        }

        public bool CheckFaceQuality(List<DisplayFaceQualityLevelItem> faceQualityLevelItems)
        {
            int conut = 0;
            for (int i = 0; i < faceQualityLevelItems.Count; i++)
            {
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Half)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Half)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Small)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Small)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Posture)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Posture)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Mask)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Mask)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.FaceMask)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.FaceMask)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Hat)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Hat)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Glasses)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Glasses)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Gape)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Gape)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Blur)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Blur)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Bright)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Bright)
                {
                    conut++;
                }
                if (faceQualityLevelItems[i].SourceName == nameof(_appSettingService.Content.FaceQualityLevel.Light)
                    && faceQualityLevelItems[i].IntValue <= _appSettingService.Content.FaceQualityLevel.Light)
                {
                    conut++;
                }
            }
            if (conut == faceQualityLevelItems.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task OnApplicationExitAsync()
        {
            _startCapture = false;
            // 关闭定时器及相机
            await StopAsync();


        }

        async Task StopAsync()
        {
            _captureTokenSource?.Cancel();

            if (_captureTask != null)
            {
                try
                {
                    await _captureTask;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logService.Error("捕获任务退出时异常", ex);
                }
            }

            _captureTokenSource?.Dispose();
            _captureTokenSource = null;
            _captureTask = null;

            if (pFrameColor != IntPtr.Zero) Marshal.FreeHGlobal(pFrameColor);
            if (pFrameGray != IntPtr.Zero) Marshal.FreeHGlobal(pFrameGray);
            if (pFrameTemp1 != IntPtr.Zero) Marshal.FreeHGlobal(pFrameTemp1);
            if (pFrameTemp2 != IntPtr.Zero) Marshal.FreeHGlobal(pFrameTemp2);

            if (bCameraReady)
            {
                bCameraReady = false;
                _captureService.CAPTURE_CLOSE(hCamColor);
                _captureService.CAPTURE_UNINIT(hCamColor);
            }

            if (bSdkInit)
            {
                bSdkInit = false;
                _ThFaceService.IdFaceUninit();
            }
        }

        int aaa = 0;
        //模拟刷卡后进行人脸拍照返回照片与检测质量
        private async void TestFaceFeatureCount()
        {
            try
            {

                if (TapCardFrameColor != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(TapCardFrameColor);
                    TapCardFrameColor = IntPtr.Zero;
                }
                if (TapCardFrameTemp != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(TapCardFrameTemp);
                    TapCardFrameTemp = IntPtr.Zero;
                }

                TapCardFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
                TapCardFrameTemp = Marshal.AllocHGlobal(1920 * 1080 * 3);

                // 获取图像帧数据
                int result = _captureService.CAPTURE_GETFRAME(hCamColor, TapCardFrameTemp, ref TapCardLen, ref TapCardWidth, ref TapCardHeight);
                int maxFaceCount = 5;

                // 分配 FACE_DETECT_RESULT 数组
                int faceStructSize = Marshal.SizeOf(typeof(FACE_DETECT_RESULT));

                // 分配非托管内存
                IntPtr pFaceResults = Marshal.AllocHGlobal(faceStructSize * maxFaceCount);

                int ret = 0;
                try
                {
                    // 调用函数
                    ret = _ThFaceService.IdFaceFaceFeature(TapCardFrameTemp, TapCardWidth, TapCardHeight, maxFaceCount, pFaceResults, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    _logService.Info("人脸数量检测异常：" + ex);
                }
                finally
                {
                    if (pFaceResults != IntPtr.Zero)
                        Marshal.FreeHGlobal(pFaceResults);
                    if (TapCardFrameColor != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(TapCardFrameColor);
                        TapCardFrameColor = IntPtr.Zero;
                    }
                    if (TapCardFrameTemp != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(TapCardFrameTemp);
                        TapCardFrameTemp = IntPtr.Zero;
                    }
                }


                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    aaa++;
                    //同步刷新到 UI 预览
                    Model.Message = "人脸数量：" + ret + "，执行次数:" + aaa;
                    //Model.FaceQualityLevelItem = displayList;
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    aaa++;
                    //同步刷新到 UI 预览
                    Model.Message = "异常：" + ex;
                    //Model.FaceQualityLevelItem = displayList;
                });
            }
        }

    }
}

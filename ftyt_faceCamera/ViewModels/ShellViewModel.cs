using ftyt_faceCamera.IService;
using ftyt_faceCamera.Models;
using ftyt_faceCamera.Service;
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
using IDialogService = ftyt_faceCamera.IService.IDialogService;
using System.Runtime.InteropServices;
using static ftyt_faceCamera.Models.FaceParameterModel;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Microsoft.Windows.Input;
using log4net.Core;
using static ftyt_faceCamera.Enums.FaceQualityLevel;
using ftyt_faceCamera.Extension;
using ftyt_faceCamera.Converters;
using ftyt_faceCamera.Enums;

namespace ftyt_faceCamera.ViewModels
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

        FACE_DETECT_RESULT FaceColor, FaceColorExt; // SDK检测到的彩色图象原始人脸坐标，以及界面显示及保存图象的人脸坐标（在原始坐标基础上根据fExtend?变量放大）
        FACE_DETECT_RESULT FaceGray, FaceGrayExt; // SDK检测到的红外图象原始人脸坐标，以及界面显示及保存图象的人脸坐标（在原始坐标基础上根据fExtend?变量放大）

        IntPtr DetectFrameColor; // 保存彩色相机和红外相机的图象帧数据用于人脸检测
        IntPtr DetectFrameTemp1; // 临时图象帧数据缓冲区用于人脸检测




        public ShellViewModel(IEventAggregator eventAggregator, DemoAppSettingService appSettingService, IRegionManager regionManager, IDialogService dialogService, IThFaceService thFaceService, LogService logService, ICaptureService captureService)
        {
            _appSettingService = appSettingService;
            _regionManager = regionManager;
            _dialogService = dialogService;


            Task.Run(() => UpdateDateTimeNow());

            _logService = logService;
            _captureService = captureService;
            _ThFaceService = thFaceService;


            InitializeCommand = new DelegateCommand(HandleInitializeAsync);
            CaptureFrameCommand = new DelegateCommand(ShowImage);
            PreviewCommand = new DelegateCommand(Preview);
            DetectFaceCommand = new DelegateCommand(SingleDetectFace);
            LiveFaceCommand = new DelegateCommand(StartDetectFace);
            DetectFaceQualityCommand = new DelegateCommand(FaceQualityLevel);
        }



        public ShellModel Model { get; } = new ShellModel();
        public ICommand InitializeCommand { get; }
        public ICommand CaptureFrameCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand DetectFaceCommand { get; }
        public ICommand LiveFaceCommand { get; }
        public ICommand DetectFaceQualityCommand { get; }



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

        async void HandleInitializeAsync()
        {
            Model.PreviewButtonName = "Preview";
            Model.LiveFaceButtonName = "StartLiveDetect";
        }


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

                        FaceGray = new FACE_DETECT_RESULT();
                        FaceGrayExt = new FACE_DETECT_RESULT();

                        bSdkInit = true;
                        // 打开彩色相机
                        hCamColor = _captureService.CAPTURE_INIT();
                        _captureService.CAPTURE_OPEN(hCamColor, _appSettingService.Content.Camera.DeviceNo);

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

                                BitmapSource imageData = null;

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
            //if (DetectFrameColor != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameColor);
            //if (DetectFrameTemp1 != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameTemp1);

            //DetectFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
            //DetectFrameTemp1 = Marshal.AllocHGlobal(1920 * 1080 * 3);



            // 获取图像帧数据
            int result = _captureService.CAPTURE_GETFRAME(hCamColor, pFrameTemp1, ref nLen, ref nWidth2, ref nHeight2);
            _logService.Info($"GETFRAME result: {result}, Length: {nLen}, Width: {nWidth2}, Height: {nHeight2}");
            if (nWidth > 0 && nHeight > 0)
            {
                // 将彩色图象数据左右镜象后放入 pFrameColor 缓冲区
                _ThFaceService.IdFaceRotateRgb24Data(pFrameTemp1, nWidth, nHeight, 0, 1, pFrameColor);
                //DetectFrameTemp1 = pFrameTemp1;
                //DetectFrameColor = pFrameColor;
            }

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

                // 保存一帧图像文件以调试
                //SaveFrameToFile(bTempBuffer, nWidth2, nHeight2, "D:\\debug_frame.bmp");

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
        private BitmapSource GetImage()
        {
            if (DetectFrameColor != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameColor);
            if (DetectFrameTemp1 != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameTemp1);

            DetectFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
            DetectFrameTemp1 = Marshal.AllocHGlobal(1920 * 1080 * 3);



            // 获取图像帧数据
            int result = _captureService.CAPTURE_GETFRAME(hCamColor, DetectFrameTemp1, ref nLen, ref nWidth2, ref nHeight2);
            _logService.Info($"GETFRAME result: {result}, Length: {nLen}, Width: {nWidth2}, Height: {nHeight2}");
            if (nWidth2 > 0 && nHeight2 > 0)
            {
                // 将彩色图象数据左右镜象后放入 pFrameColor 缓冲区
                _ThFaceService.IdFaceRotateRgb24Data(DetectFrameTemp1, nWidth2, nHeight2, 0, 1, DetectFrameColor);
            }

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

                // 保存一帧图像文件以调试
                //SaveFrameToFile(bTempBuffer, nWidth2, nHeight2, "D:\\debug_frame.bmp");

                // 返回一帧图像
                var bitmap = CreateBitmapFromBuffer(bTempBuffer, nWidth2, nHeight2);
                return bitmap;
            }
            else
            {
                return null;
            }
        }

        private void FaceQualityLevel()
        {
            try
            {

                FACE_QUALITY_LEVEL FaceQualityLevel = new FACE_QUALITY_LEVEL();
                FACE_DETECT_RESULT LevelFaceColor = new FACE_DETECT_RESULT();

                int nNum1 = _ThFaceService.IdFaceDetectFace(DetectFrameColor, nWidth2, nHeight2, ref LevelFaceColor);
                if (nNum1 < 1)
                { // 彩色镜头未检测到人脸，清除彩色人脸坐标
                    LevelFaceColor.rcFace.left = LevelFaceColor.rcFace.right = 0;
                    LevelFaceColor.rcFace.top = LevelFaceColor.rcFace.bottom = nHeight2;
                    Model.Message = "未检测到人脸";
                }
                else
                {
                    int fFaceQuality = _ThFaceService.IdFaceFaceQualityLevel(DetectFrameColor, nWidth2, nHeight2, ref LevelFaceColor, ref FaceQualityLevel);
                    if (fFaceQuality == 0)
                    {
                        var converter = new FaceQualityEnumConverter(useChinese: _appSettingService.Content.Shell.UseChineseEnumDescriptions);
                        List<DisplayFaceQualityLevelItem> displayList = converter.Convert(FaceQualityLevel);
                        Model.FaceQualityLevelItem = displayList;
                    }
                    else
                    {
                        Model.Message = "Face Quality Level Detect Fial";
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Info($"[FaceQualityLevel] {ex}");
            }
        }

        public void ShowImage()
        {
            try
            {
                if (isPreview == false)
                {
                    Model.Message = "Please start preview";
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //同步刷新到 UI 预览
                    CaptureImage = GetImage();
                });
            }
            catch (Exception ex)
            {
                _logService.Info($"[ShowImage] {ex}");
            }

        }

        public void SaveFrameToFile(byte[] buffer, int width, int height, string filePath)
        {
            using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                var bmpData = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
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

        private void SingleDetectFace()
        {
            try
            {

                Model.Message = DetectFace();
            }
            catch (Exception ex)
            {
                _logService.Info($"[SingleDetectFace] {ex}");
            }
        }

        readonly object _detectLock = new object();
        async void StartDetectFace()
        {
            try
            {

                if (!isDetectFace)
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
                            StringBuilder message = new StringBuilder();
                            List<DisplayFaceQualityLevelItem> displayList;
                            lock (_detectLock)
                            {
                                var (messageInfo, list) = AutoDetectFace();
                                message.Append(messageInfo.ToString());
                                displayList = list == null ? new List<DisplayFaceQualityLevelItem>(): list;
                            }
                            try
                            {
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    //同步刷新到 UI 预览
                                    Model.Message = message.ToString();
                                    Model.FaceQualityLevelItem = displayList;
                                });
                                try
                                {
                                    await Task.Delay(500, detecttoken); // 控制帧率
                                }
                                catch (TaskCanceledException)
                                {
                                    break; // 安全退出循环
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.Error("人脸检测异常", ex);
                            }
                        }
                    }, detecttoken);
                    isDetectFace = true;
                    Model.LiveFaceButtonName = "StopLiveDetect";
                }
                else
                {
                    isDetectFace = false;
                    _detectTokenSource?.Cancel();
                    if (_detectTask != null)
                    {
                        try
                        {
                            await _detectTask;
                        }
                        catch (TaskCanceledException) { }
                        _detectTask = null;
                    }
                    isDetectFace = false;
                    Model.LiveFaceButtonName = "StartLiveDetect";
                    Model.Message = "";
                }
            }
            catch (Exception ex)
            {
                _logService.Info($"[StartDetectFace] {ex}");
            }
        }

        //活体检测
        private string DetectFace()
        {
            nFrameDetect++;

            // 分配彩色原始人脸坐标空间、彩色放大人脸坐标空间、红外原始人脸坐标空间、红外放大人脸坐标空间
            FACE_DETECT_RESULT FaceColor = new FACE_DETECT_RESULT();
            FaceColorExt = new FACE_DETECT_RESULT();

            // 对彩色帧图象分别检测人脸
            int nNum1 = _ThFaceService.IdFaceDetectFace(DetectFrameColor, nWidth2, nHeight2, ref FaceColor);

            if (nNum1 < 1)
            { // 彩色镜头未检测到人脸，清除彩色人脸坐标
                FaceColor.rcFace.left = FaceColor.rcFace.right = 0;
                FaceColor.rcFace.top = FaceColor.rcFace.bottom = nHeight2;
                FaceColorExt = FaceColor;
            }
            else
            { // 彩色镜头检测到人脸，更新彩色人脸放大坐标
                int w = FaceColor.rcFace.right - FaceColor.rcFace.left, h = FaceColor.rcFace.bottom - FaceColor.rcFace.top;
                int left = FaceColor.rcFace.left - (int)(w * fExtendLeft), right = FaceColor.rcFace.right + (int)(w * fExtendRight), top = FaceColor.rcFace.top - (int)(h * fExtendTop), bottom = FaceColor.rcFace.bottom + (int)(h * fExtendBottom);
                if (left < 0) left = 0;
                if (right >= nWidth2) right = nWidth2 - 1;
                if (top < 0) top = 0;
                if (bottom >= nHeight2) bottom = nHeight2 - 1;

                FaceColorExt.rcFace.left = left;
                FaceColorExt.rcFace.top = top;
                FaceColorExt.rcFace.right = right;
                FaceColorExt.rcFace.bottom = bottom;

            }

            if (nNum1 > 0) // 至少有一个相机能检测到人脸时，进行活体判别
            {
                int nLiveFace = 0;

                int nScore = 0;

                // 如果彩色和红外相机选择的是同一个，则调用彩色图象单目人脸检测，否则调用彩色及红外图象双目人脸检测
                nLiveFace = _ThFaceService.IdFaceLiveFaceDetectEx(nWidth2, nHeight2, DetectFrameColor, ref FaceColor, (IntPtr)0, ref FaceGray, ref nScore);

                if (nLiveFace == 1)
                {
                    return "确认为活体，分数 " + nScore.ToString();
                }
                else if (nLiveFace == 0)
                {
                    return "未确认为活体，分数 " + nScore.ToString();
                }
                else
                {
                    return "活体检测失败，接口返回 " + nLiveFace.ToString();
                }


                if (false) // 是否保存人脸图片，每帧人脸图片都保存会占用较大磁盘空间
                {
                    // 保存人脸图片，特别注意 CHS_CAPTURE_SAVEBMPFILE 要求输入的图象数据是倒立的（头朝下脚朝上），但人脸坐标仍相对于正立图象

                    string strTimeFlag = System.DateTime.Now.ToString("yyyyMMddHHmmss");

                    string strFolder = Environment.CurrentDirectory + "\\Record";
                    if (System.IO.Directory.Exists(strFolder) == false)
                        System.IO.Directory.CreateDirectory(strFolder);

                    string strFileNameColor = strFolder + "\\" + strTimeFlag + "_" + nLiveFace.ToString() + "_1.bmp";
                    IntPtr ptrFileNameColor = Marshal.StringToHGlobalAnsi(strFileNameColor);
                    _ThFaceService.IdFaceRotateRgb24Data(pFrameColor, nWidth2, nHeight2, 180, 1, pFrameTemp2);
                    _captureService.CAPTURE_SAVEBMPFILE(ptrFileNameColor, pFrameTemp2, nWidth2, nHeight2, FaceColorExt.rcFace.left, FaceColorExt.rcFace.top, FaceColorExt.rcFace.right, FaceColorExt.rcFace.bottom);
                }
            }
            else
            {
                // 未检测到人脸
                return "请正视摄像头！";
            }
        }

        //活体检测
        private (string, List<DisplayFaceQualityLevelItem>?) AutoDetectFace()
        {
            List<DisplayFaceQualityLevelItem> displayList = null;
            string messageInfo = "";

            if (DetectFrameColor != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameColor);
            if (DetectFrameTemp1 != IntPtr.Zero) Marshal.FreeHGlobal(DetectFrameTemp1);

            DetectFrameColor = Marshal.AllocHGlobal(1920 * 1080 * 3);
            DetectFrameTemp1 = Marshal.AllocHGlobal(1920 * 1080 * 3);



            // 获取图像帧数据
            int result = _captureService.CAPTURE_GETFRAME(hCamColor, DetectFrameTemp1, ref nLen, ref nWidth2, ref nHeight2);
            _logService.Info($"GETFRAME result: {result}, Length: {nLen}, Width: {nWidth2}, Height: {nHeight2}");
            if (nWidth2 > 0 && nHeight2 > 0)
            {
                // 将彩色图象数据左右镜象后放入 pFrameColor 缓冲区
                _ThFaceService.IdFaceRotateRgb24Data(DetectFrameTemp1, nWidth2, nHeight2, 0, 1, DetectFrameColor);
            }
            // 分配彩色原始人脸坐标空间、彩色放大人脸坐标空间、红外原始人脸坐标空间、红外放大人脸坐标空间
            FACE_DETECT_RESULT DetectFaceColor = new FACE_DETECT_RESULT();
            FACE_DETECT_RESULT DetectFaceColorExt = new FACE_DETECT_RESULT();

            // 对彩色帧图象分别检测人脸
            int nNum1 = _ThFaceService.IdFaceDetectFace(DetectFrameColor, nWidth2, nHeight2, ref DetectFaceColor);

            if (nNum1 < 1)
            { // 彩色镜头未检测到人脸，清除彩色人脸坐标
                DetectFaceColor.rcFace.left = DetectFaceColor.rcFace.right = 0;
                DetectFaceColor.rcFace.top = DetectFaceColor.rcFace.bottom = nHeight2;
                DetectFaceColorExt = DetectFaceColor;
            }
            else
            { // 彩色镜头检测到人脸，更新彩色人脸放大坐标
                int w = DetectFaceColor.rcFace.right - DetectFaceColor.rcFace.left, h = DetectFaceColor.rcFace.bottom - DetectFaceColor.rcFace.top;
                int left = DetectFaceColor.rcFace.left - (int)(w * fExtendLeft), right = DetectFaceColor.rcFace.right + (int)(w * fExtendRight), top = DetectFaceColor.rcFace.top - (int)(h * fExtendTop), bottom = DetectFaceColor.rcFace.bottom + (int)(h * fExtendBottom);
                if (left < 0) left = 0;
                if (right >= nWidth2) right = nWidth2 - 1;
                if (top < 0) top = 0;
                if (bottom >= nHeight2) bottom = nHeight2 - 1;

                DetectFaceColorExt.rcFace.left = left;
                DetectFaceColorExt.rcFace.top = top;
                DetectFaceColorExt.rcFace.right = right;
                DetectFaceColorExt.rcFace.bottom = bottom;

            }

            if (nNum1 > 0) // 至少有一个相机能检测到人脸时，进行活体判别
            {

                FACE_QUALITY_LEVEL FaceQualityLevel = new FACE_QUALITY_LEVEL();
                int fFaceQuality = _ThFaceService.IdFaceFaceQualityLevel(DetectFrameColor, nWidth2, nHeight2, ref DetectFaceColor, ref FaceQualityLevel);
                if (fFaceQuality == 0)
                {
                    var converter = new FaceQualityEnumConverter(useChinese: _appSettingService.Content.Shell.UseChineseEnumDescriptions);
                    displayList = converter.Convert(FaceQualityLevel);

                    int nLiveFace = 0;

                    int nScore = 0;

                    // 如果彩色和红外相机选择的是同一个，则调用彩色图象单目人脸检测，否则调用彩色及红外图象双目人脸检测
                    nLiveFace = _ThFaceService.IdFaceLiveFaceDetectEx(nWidth2, nHeight2, DetectFrameColor, ref DetectFaceColor, (IntPtr)0, ref FaceGray, ref nScore);

                    if (nLiveFace == 1)
                    {
                        messageInfo = "确认为活体，分数 " + nScore.ToString();
                    }
                    else if (nLiveFace == 0)
                    {
                        messageInfo = "未确认为活体，分数 " + nScore.ToString();
                    }
                    else
                    {
                        messageInfo = "活体检测失败，接口返回 " + nLiveFace.ToString();
                    }
                }
                else
                {
                    messageInfo = "Face Quality Level Detect Fial";
                }
            }
            else
            {
                // 未检测到人脸
                messageInfo = "请正视摄像头！";
            }
            return (messageInfo, displayList);
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
    }
}

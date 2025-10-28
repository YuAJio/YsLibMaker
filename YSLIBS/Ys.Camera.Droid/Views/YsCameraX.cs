using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;

using Java.Nio;
using Java.Util.Concurrent;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ys.Camera.Droid.Implements;

namespace Ys.Camera.Droid.Views
{
    /// <summary>
    /// 自动切换大小的摄像头承载控件
    /// </summary>
    public class YsCameraX : FrameLayout
    {
        #region 构造方法
        public YsCameraX(Context context) : base(context)
        {
            InitInfo();
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            InitInfo();
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            InitInfo();
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            InitInfo();
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        protected YsCameraX(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            InitInfo();
            AddCameraView();
        }
        #endregion


        private void InitInfo()
        {
            var boardInfo = Android.OS.Build.Device;
            if (boardInfo == "rk3568_r")//如果是这个,则代表是25年初梁工的板子,需要设置默认摄像头方向是1
                boardVersion = 1;
            else
                boardVersion = 0;//反转了,都是1

            //通过主板型号配置默认摄像头坐标
            //if (boardVersion == 1)//如果是这个,则代表是25年初梁工的板子,需要设置默认摄像头方向是1
            CameraIndex = 1;//好像都是1诶
                            // 初始化缩放范围
            zoomRange[0] = 1.0f;  // 默认最小值
            zoomRange[1] = 1.0f;  // 默认最大值
        }

        public event Action<bool, string> PhotoTakeEvent;

        /// <summary>
        /// 初始化添加CameraView进布局
        /// </summary>
        private void AddCameraView()
        {
            _CameraPreView = new PreviewView(this.Context)
            {
                LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) { Gravity = GravityFlags.Start }
            };
            _CameraPreView.SetImplementationMode(PreviewView.ImplementationMode.Compatible);
            if (boardVersion == 1)//兼容处理U称主板和万创主板
                _CameraPreView.SetScaleType(PreviewView.ScaleType.FillStart);
            else
                _CameraPreView.SetScaleType(PreviewView.ScaleType.FillEnd);

            //_CameraPreView.SetLayerType(LayerType.Hardware, null);
            this.AddView(_CameraPreView, 0);
        }

        #region 定义的自定义属性 
        private int CaptureImageSize_Width = 1280;
        private int CaptureImageSize_Height = 720;
        /// <summary>
        /// 主板版本
        /// 0:万创和其他
        /// 1:梁工
        /// </summary>
        private int boardVersion = 0;
        public int CameraIndex { get; private set; }

        private void SetDiyParams(Context context, IAttributeSet attrs)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.YsCameraX);
            CaptureImageSize_Width = a.GetInt(Resource.Styleable.YsCameraX_CapturePictureSize_Width, 1280);
            CaptureImageSize_Height = a.GetInt(Resource.Styleable.YsCameraX_CapturePictureSize_Height, 720);
            CameraIndex = a.GetInt(Resource.Styleable.YsCameraX_Camera_Facing, CameraIndex);
            a.Recycle();
        }

        #endregion

        #region 缩进控制

        public float GetCurrentZoom()
        {
            return currentZoomRatio;
        }
        #endregion

        private PreviewView _CameraPreView;
        private UseCaseGroup _CameraUseCases;
        private ProcessCameraProvider _cameraProvider;
        private ICameraControl _CameraController;
        private ICameraInfo _CameraInfo;
        private ImageCapture imageCapture;
        /// <summary>
        /// 初始化并绑定摄像头
        /// </summary>
        /// <param name="lifecycleOwner"></param>
        public void InitAndStartCamera(ILifecycleOwner lifecycleOwner, Action<bool, Exception> InitCallBack)
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(this.Context);
            cameraExecutor = Executors.NewSingleThreadExecutor();
            cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
            {
                try
                {
                    // Used to bind the lifecycle of cameras to the lifecycle owner
                    _cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();

                    var imageAnalyzer = new ImageAnalysis.Builder().Build();
                    imageAnalysisFrameProcess = new ImageAnalysisFrameProcess();
                    //imageAnalysisFrameProcess.ImageFrameCaptured -= ImageAnalysisFrameProcess_ImageFrameCaptured;
                    //imageAnalysisFrameProcess.ImageFrameCaptured += ImageAnalysisFrameProcess_ImageFrameCaptured;
                    imageAnalyzer.SetAnalyzer(cameraExecutor, imageAnalysisFrameProcess);

                    CameraSelector cameraSelector = null;

                    #region 新摄像头坐标选择方案
                    cameraSelector = new CameraSelector.Builder()
                    .AddCameraFilter(new YsCameraFilter(CameraIndex))
                    .Build();
                    #endregion

                    // Preview
                    var preview = new Preview.Builder()
                    //.SetTargetAspectRatio(AspectRatio.Ratio43)
                    .Build();
                    preview.SetSurfaceProvider(_CameraPreView.SurfaceProvider);

                    // 在 BindToLifecycle 之前构建 ImageCapture
                    imageCapture = new ImageCapture.Builder()
                        .SetTargetResolution(new Android.Util.Size(CaptureImageSize_Width, CaptureImageSize_Height))
                        .SetIoExecutor(cameraExecutor)  // 关键：用单线程避免 IO 竞争
                        .SetJpegQuality(85)
                        .Build();

                    // Unbind use cases before rebinding
                    _cameraProvider?.UnbindAll();
                    // Bind use cases to camera
                    //_CameraUseCases = new UseCaseGroup.Builder()
                    //.AddUseCase(preview)
                    //.AddUseCase(imageCapture)
                    //.AddUseCase(imageAnalyzer).Build();
                    //var camera = cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, _CameraUseCases);
                    var camera = _cameraProvider?.BindToLifecycle(
                        lifecycleOwner,
                        cameraSelector,
                        preview,
                        imageCapture,
                        imageAnalyzer);
                    _CameraController = camera.CameraControl;
                    _CameraInfo = camera.CameraInfo;

                    var zoomValue = camera?.CameraInfo?.ZoomState?.Value;
                    if (zoomValue != null && zoomValue is IZoomState)//如果不支持缩进 这里就先排除掉转换问题
                        SetZoomValue((IZoomState)camera.CameraInfo.ZoomState.Value);
                    InitCallBack?.Invoke(true, null);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Camera Init Error : " + exc.Message);
                    InitCallBack?.Invoke(false, exc);
                }
            }), AndroidX.Core.Content.ContextCompat.GetMainExecutor(this.Context));
        }

        // 新增：解绑相机方法
        public void UnbindCamera()
        {
            if (_CameraUseCases != null)
            {
                // 假设你有ProcessCameraProvider实例（可全局化或传入）
                _cameraProvider?.UnbindAll();  // 解绑所有用例
            }
            if (cameraExecutor != null)
            {
                cameraExecutor.Shutdown();  // 关闭执行器
            }
            ImageAnalysisFrameProcess?.Dispose();  // 如果有Dispose
        }

        //设置指定的Index
        public void SetCustomCameraIndex(int index)
        {
            this.CameraIndex = index;
        }

        private void ImageAnalysisFrameProcess_ImageFrameCaptured(object sender, ImageFrameArgs e)
        {
            //if (imageCaptureStep == 1)
            //{
            //    // 添加：检查Context是否有效
            //    if (Context == null) return;

            //    imageCaptureStep = 2;
            //    var bitmap = ToBitmap(e.imageProxy.Image);
            //    if (bitmap != null)
            //    {
            //        if (boardVersion == 0)//如果是老版本则需要镜像一下拍摄画面,新版本不需要
            //        {
            //            #region 处理图片翻转
            //            var matrix = new Matrix();
            //            matrix.PostScale(-1f, -1f);
            //            var newBitMap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);
            //            #endregion
            //        }
            //        if (File.Exists(ImageCapture_ImagePath))
            //            File.Delete(ImageCapture_ImagePath);
            //        using var stream = new FileStream(ImageCapture_ImagePath, FileMode.Create);
            //        bitmap.Compress(Bitmap.CompressFormat.Png, 80, stream); // 以PNG格式保存Bitmap
            //        PhotoTakeEvent?.Invoke(true, ImageCapture_ImagePath);
            //    }
            //    else
            //        PhotoTakeEvent?.Invoke(false, "拍照失败,请重试");
            //    imageCaptureStep = 0;
            //}
        }


        /// <summary>
        /// 切换摄像头的位置
        /// </summary>
        public void SwitchFacing(ILifecycleOwner lifecycleOwner, Action<bool, Exception> switchCallBack)
        {
            CameraIndex = CameraIndex == 0 ? 1 : 0;
            InitAndStartCamera(lifecycleOwner, switchCallBack);
        }



        #region 捕获实时帧相关
        private IExecutorService cameraExecutor;
        private ImageAnalysisFrameProcess imageAnalysisFrameProcess;
        public ImageAnalysisFrameProcess ImageAnalysisFrameProcess { get { return imageAnalysisFrameProcess; } private set { imageAnalysisFrameProcess = value; } }

        [Obsolete]
        public void OpenFrameCapture()
        {
            //if (ImageAnalysisFrameProcess == null) return;
            //ImageAnalysisFrameProcess.IsOpenFrameCapture = true;
        }

        [Obsolete]
        public void CloseFrameCapture()
        {
            //if (ImageAnalysisFrameProcess == null) return;
            //ImageAnalysisFrameProcess.IsOpenFrameCapture = false;
        }

        public void EnableAIClassify()
        {
            if (ImageAnalysisFrameProcess == null) return;
            ImageAnalysisFrameProcess.EnableAIDetect = true;
        }
        public void DisableAIClassify()
        {
            if (ImageAnalysisFrameProcess == null) return;
            ImageAnalysisFrameProcess.EnableAIDetect = false;
        }
        #endregion

        #region Zoom缩放相关
        //缩放控制
        private float currentZoomRatio = 1.0f;
        private readonly float[] zoomRange = new float[2];
        /// <summary>
        /// 设置缩放
        /// </summary>
        /// <param name="zoomPercent"></param>
        public void SetZoom(float zoomPercent)
        {
            try
            {
                if (_CameraController != null)
                {
                    zoomPercent = Math.Max(zoomRange[0], Math.Min(zoomPercent, zoomRange[1]));
                    _CameraController.SetZoomRatio(zoomPercent);
                }
            }
            catch (Exception ex)
            {
                // 错误处理...
            }
        }
        /// <summary>
        /// 获取相机是否支持缩进
        /// </summary>
        /// <returns>first : 是否支持  second : 缩进的最大值</returns>
        public (bool, float) GetZoomEnable()
        {
            if (zoomRange.Any())
            {
                if (zoomRange.Count() > 1)
                {
                    if (zoomRange.First() == zoomRange.Last())//如果获取的
                        return (false, 0);//如果获取的缩进最大值和最小值都是一个值,那也不支持缩进
                    else
                        return (true, zoomRange.Last());//支持缩进
                }
                else
                    return (false, 0);///如果只获取到一个值,也不支持缩进
            }
            else
                return (false, 0);
        }

        private void SetZoomValue(IZoomState zoomState)
        {
            // 获取实际缩放范围
            zoomRange[0] = zoomState.MinZoomRatio;
            zoomRange[1] = zoomState.MaxZoomRatio;
            currentZoomRatio = zoomState.ZoomRatio;
        }
        #endregion

        #region 使用照片帧截取拍照相关
        /// <summary>
        /// 拍摄的图片的路径
        /// </summary>
        private string ImageCapture_ImagePath = "";
        /// <summary>
        /// 拍照步骤
        /// 0:待机中
        /// 1:开启拍摄
        /// 2:拍摄中
        /// </summary>
        private int imageCaptureStep = 0;

        ///// <summary>
        ///// 开始照片拍摄
        ///// </summary>
        //public void StartPhotoTaking(string savePath)
        //{
        //    if (imageCaptureStep != 0)
        //    {
        //        PhotoTakeEvent?.Invoke(false, "照片拍摄中..请稍等");
        //        return;
        //    }
        //    SetCapturePicturePath(savePath);
        //    imageCaptureStep = 1;
        //}
        public void StartPhotoTaking(string savePath)
        {
            if (imageCapture == null || imageCaptureStep != 0)
            {
                PhotoTakeEvent?.Invoke(false, "相机忙碌中");
                return;
            }

            imageCaptureStep = 1;

            // 强制用 cache 目录
            var fileName = System.IO.Path.GetFileName(savePath);
            var cachePath = System.IO.Path.Combine(Context.CacheDir.Path, "camx_" + fileName);
            ImageCapture_ImagePath = cachePath;

            var file = new Java.IO.File(cachePath);
            var outputOptions = new ImageCapture.OutputFileOptions.Builder(file).Build();

            imageCapture.TakePicture(outputOptions, cameraExecutor, new ImageSavedCallback(this, savePath));
        }
        #endregion

        /// <summary>
        /// 配置保存的照片路径
        /// </summary>
        /// <param name="path"></param>
        public void SetCapturePicturePath(string path)
        {
            //ImageCapture_ImagePath = path;
            //var fileInfo = new System.IO.FileInfo(path);
            //if (path == null)
            //    return;
            //if (!fileInfo.Directory.Exists)
            //    fileInfo.Directory.Create();
            var cacheDir = Context.CacheDir.Path;
            var fileName = System.IO.Path.GetFileName(path);
            ImageCapture_ImagePath = System.IO.Path.Combine(cacheDir, "camx_" + fileName);
        }
        private void MoveToFinalPath(string cachePath, string finalPath)
        {
            Task.Run(() =>
            {
                try
                {
                    System.IO.File.Copy(cachePath, finalPath);

                    //var src = new Java.IO.File(cachePath);
                    //var dst = new Java.IO.File(finalPath);
                    //if (!dst.ParentFile.Exists()) dst.ParentFile.Mkdirs();


                    //CopyFile(src, dst);
                    //InsertToMediaStore(finalPath);  // 关键！
                    File.Delete(cachePath);
                    //src.Delete();

                    PhotoTakeEvent?.Invoke(true, finalPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }
        private void CopyFile(Java.IO.File src, Java.IO.File dst)
        {
            using var input = new Java.IO.FileInputStream(src);
            using var output = new Java.IO.FileOutputStream(dst);
            input.Channel.TransferTo(0, input.Channel.Size(), output.Channel);
        }

        private void InsertToMediaStore(string filePath)
        {
            MediaScannerConnection.ScanFile(
                  Context,
                  new[] { filePath },
                  new[] { "image/jpeg" },
                  null
              );
        }

        //private Bitmap ToBitmap(Image image)
        //{
        //    // 假设planes是Image.GetPlanes()获取的Image.Plane数组
        //    ByteBuffer yBuffer = image.GetPlanes()[0].Buffer; // Y
        //    ByteBuffer vuBuffer = image.GetPlanes()[2].Buffer; // VU

        //    int ySize = yBuffer.Remaining();
        //    int vuSize = vuBuffer.Remaining();

        //    byte[] nv21 = new byte[ySize + vuSize];

        //    yBuffer.Get(nv21, 0, ySize);
        //    vuBuffer.Get(nv21, ySize, vuSize);

        //    YuvImage yuvImage = new YuvImage(nv21, ImageFormatType.Nv21, image.Width, image.Height, null);
        //    using (MemoryStream outStream = new MemoryStream())
        //    {
        //        yuvImage.CompressToJpeg(new Rect(0, 0, yuvImage.Width, yuvImage.Height), 50, outStream);
        //        byte[] imageBytes = outStream.ToArray();
        //        return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
        //    }
        //}

        private class ImageSavedCallback : Java.Lang.Object, ImageCapture.IOnImageSavedCallback
        {
            private readonly WeakReference<YsCameraX> _parent;
            private readonly string _finalPath;

            public ImageSavedCallback(YsCameraX parent, string finalPath)
            {
                _parent = new WeakReference<YsCameraX>(parent);
                _finalPath = finalPath;
            }

            public void OnImageSaved(ImageCapture.OutputFileResults results)
            {
                if (_parent.TryGetTarget(out var p))
                {
                    p.imageCaptureStep = 0;
                    p.MoveToFinalPath(p.ImageCapture_ImagePath, _finalPath);
                }
            }

            public void OnError(ImageCaptureException e)
            {
                if (_parent.TryGetTarget(out var p))
                {
                    p.imageCaptureStep = 0;
                    p.PhotoTakeEvent?.Invoke(false, e.Message);
                }
            }
        }
    }
}
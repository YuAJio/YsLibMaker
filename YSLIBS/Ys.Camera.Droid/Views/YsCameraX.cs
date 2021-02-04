using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Ys.Camera.Droid.Enums;

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
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        public YsCameraX(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            SetDiyParams(context, attrs);
            AddCameraView();
        }

        protected YsCameraX(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            AddCameraView();
        }
        #endregion

        /// <summary>
        /// 初始化添加CameraView进布局
        /// </summary>
        private void AddCameraView()
        {
            _CameraPreView = new PreviewView(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            this.AddView(_CameraPreView, 0);
        }

        #region 定义的自定义属性 
        private int CaptureImageSize_Width = 1280;
        private int CaptureImageSize_Height = 720;
        private CameraFacing _CameraFacing = CameraFacing.Back;

        private void SetDiyParams(Context context, IAttributeSet attrs)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.YsCameraX);
            CaptureImageSize_Width = a.GetInt(Resource.Styleable.YsCameraX_CapturePictureSize_Width, 0);
            CaptureImageSize_Height = a.GetInt(Resource.Styleable.YsCameraX_CapturePictureSize_Height, 0);
            _CameraFacing = (CameraFacing)a.GetInt(Resource.Styleable.YsCameraX_Camera_Facing, 0);
            a.Recycle();
        }

        #endregion


        private PreviewView _CameraPreView;
        private ImageCapture _ImageCapture;
        #region 摄像头创建相关

        /// <summary>
        /// 初始化并绑定摄像头
        /// </summary>
        /// <param name="lifecycleOwner"></param>
        public void InitAndStartCamera(ILifecycleOwner lifecycleOwner)
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(this.Context);

            cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
            {
                // Used to bind the lifecycle of cameras to the lifecycle owner
                var cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();

                // Preview
                var preview = new Preview.Builder()
                .Build();
                preview.SetSurfaceProvider(_CameraPreView.SurfaceProvider);

                // Take Photo
                this._ImageCapture = new ImageCapture.Builder()
                .SetTargetResolution(new Size(CaptureImageSize_Width, CaptureImageSize_Height))
                .Build();

                // Frame by frame analyze(Not Use Now)
                var imageAnalyzer = new ImageAnalysis.Builder().Build();
                //imageAnalyzer.SetAnalyzer(cameraExecutor, new LuminosityAnalyzer(luma =>
                //    Log.Debug(TAG, $"Average luminosity: {luma}")
                //    ));

                #region Select back camera as a default, or front camera otherwise
                CameraSelector cameraSelector = null;
                //if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera) == true)
                //    cameraSelector = CameraSelector.DefaultBackCamera;
                //else if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera) == true)
                //    cameraSelector = CameraSelector.DefaultFrontCamera;
                //else
                //    throw new System.Exception("Camera not found");
                switch (_CameraFacing)
                {
                    case CameraFacing.Back:
                        if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera) == true)
                            cameraSelector = CameraSelector.DefaultBackCamera;
                        else
                        {
                            if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera) == true)
                                cameraSelector = CameraSelector.DefaultFrontCamera;
                            else
                                throw new Exception("Not found any camera device");
                        }
                        break;
                    case CameraFacing.Front:
                        {
                            if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera) == true)
                                cameraSelector = CameraSelector.DefaultFrontCamera;
                            else
                            {
                                if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera) == true)
                                    cameraSelector = CameraSelector.DefaultBackCamera;
                                else
                                    throw new Exception("Not found any camera device");
                            }
                        }
                        break;
                }
                #endregion
                try
                {
                    // Unbind use cases before rebinding
                    cameraProvider.UnbindAll();

                    // Bind use cases to camera
                    cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, preview, _ImageCapture, imageAnalyzer);
                }
                catch (Exception exc)
                {
                    Toast.MakeText(this.Context, $"Use case binding failed: {exc.Message}", ToastLength.Short).Show();
                }
            }), AndroidX.Core.Content.ContextCompat.GetMainExecutor(this.Context));


        }

        ///// <summary>
        ///// 释放执行器
        ///// 推荐在Destory中调用
        ///// </summary>
        //private void RelaseExcute()
        //{
        //    //暂无需要销毁的东西
        //}

        #endregion

        #region 拍照相关
        /// <summary>
        /// 拍摄的图片的路径
        /// </summary>
        private string ImageCapture_ImagePath = "";

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="onError">失败回调</param>
        /// <param name="onSaved">成功回调</param>
        public void TakePicture(Action<ImageCaptureException> onError, Action<ImageCapture.OutputFileResults> onSaved)
        {
            var fileInfo = new System.IO.FileInfo(ImageCapture_ImagePath);
            if (fileInfo == null || !fileInfo.Directory.Exists)
                onError?.Invoke(new ImageCaptureException(0x123, "File dir is not exist", null));
            var outputOptions = new ImageCapture.OutputFileOptions.Builder(new Java.IO.File(ImageCapture_ImagePath)).Build();
            _ImageCapture?.TakePicture(
                outputOptions,
                AndroidX.Core.Content.ContextCompat.GetMainExecutor(this.Context),
                new Implements.ImageCaptureSave(onError, onSaved));
        }
        /// <summary>
        /// 配置保存的照片路径
        /// </summary>
        /// <param name="path"></param>
        public void SetCapturePicturePath(string path)
        {
            ImageCapture_ImagePath = path;
            var fileInfo = new System.IO.FileInfo(path);
            if (path == null)
                return;
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
        }
        /// <summary>
        /// 拍照并配置保存照片的位置
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="onError"></param>
        /// <param name="onSaved"></param>
        public void TakePicture(string savePath, Action<ImageCaptureException> onError, Action<ImageCapture.OutputFileResults> onSaved)
        {
            SetCapturePicturePath(savePath);
            this.TakePicture(onError, onSaved);
        }
        #endregion


    }
}
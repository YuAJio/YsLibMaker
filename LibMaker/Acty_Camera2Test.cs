using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Util;

namespace LibMaker
{
    [Activity(Label = "Acty_Camera2Test")]
    public class Acty_Camera2Test : Activity
    {
        private FrameLayout fl_Fahter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.acty_camera2test);
            fl_Fahter = FindViewById<FrameLayout>(Resource.Id.fl_father);
        }

        protected override void OnResume()
        {
            base.OnResume();
            var camera = new YsCameraSurfaceViewPK(this);
            camera.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            fl_Fahter.AddView(camera);

            //camera.StartCamera();
        }

    }



    public class YsCameraSurfaceViewPK : TextureView, TextureView.ISurfaceTextureListener
    {
        public enum CameraFacing
        {
            BACK = 0,
            FRONT = 1,
            EXTERNAL = 2,
        }
        private CameraFacing choseCameraFacing;

        #region SurfaceView 构造方法
        public YsCameraSurfaceViewPK(Context context) : base(context)
        {
            InitThisView();
        }

        public YsCameraSurfaceViewPK(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            InitThisView();
        }

        public YsCameraSurfaceViewPK(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            InitThisView();
        }

        public YsCameraSurfaceViewPK(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            InitThisView();
        }

        protected YsCameraSurfaceViewPK(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            InitThisView();
        }
        #endregion


        private void InitThisView()
        {
            this.SurfaceTextureListener = this;
        }

        #region 外部调用方法 
        /// <summary>
        /// 开始摄像头
        /// </summary>
        public void StartCamera()
        {
            //Task.Run(() =>
            //{
            InitCamera();
            //}).ContinueWith(x =>
            //{
            //    if (x.Exception != null)
            //    {

            //    }
            StartPreview();
            //}, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region TextureViewListener接口实现
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            ConfigureTransform(width, height);

            StartCamera();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {

        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

        }
        #endregion

        #region TextureView方法相关
        private Size previewSize;

        public void StartPreview()
        {
            if (CameraDevice == null || !this.IsAvailable || previewSize == null) return;

            var texture = this.SurfaceTexture;

            texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
            var surface = new Surface(texture);

            CaptureRequestBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
            CaptureRequestBuilder.AddTarget(surface);

            CameraDevice.CreateCaptureSession(new List<Surface> { surface },
               GetYsCameraCaptureSessionStateCallback(),
                null);

        }

        private void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (this.SurfaceTexture == null || previewSize == null || this.Context == null) return;

            var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

            var rotation = windowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Width, previewSize.Height);

            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();

            if (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)
            {
                bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);

                matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
            }

            this.SetTransform(matrix);
        }

        private void UpdatePreview()
        {
            if (CameraDevice == null || CameraCaptureSession == null) return;

            CaptureRequestBuilder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));
            var thread = new HandlerThread("CameraPreview");
            thread.Start();
            var backgroundHandler = new Handler(thread.Looper);

            CameraCaptureSession.SetRepeatingRequest(CaptureRequestBuilder.Build(), null, backgroundHandler);
        }
        #endregion

        #region 摄像头相关
        private CameraManager CameraManager;
        private CameraCharacteristics CameraCharacteristics;
        private CameraCaptureSession CameraCaptureSession;
        private CameraDevice CameraDevice;
        private CaptureRequest.Builder CaptureRequestBuilder;

        /// <summary>
        /// 初始化摄像头
        /// </summary>
        private void InitCamera()
        {
            try
            {
                CameraManager = (CameraManager)Context.GetSystemService(Context.CameraService);
                var cameraId = ((int)choseCameraFacing).ToString();
                this.CameraCharacteristics = CameraManager.GetCameraCharacteristics(cameraId);
                var jk = new Java.Lang.Integer((int)CameraCharacteristics.Get(CameraCharacteristics.InfoSupportedHardwareLevel));
                var map = (StreamConfigurationMap)CameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                previewSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
                CameraManager.OpenCamera(cameraId, GetCameraStateCallBack(), null);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 拍照
        /// </summary>
        private void TakeAShot()
        {
            if (CameraDevice == null)
                return;


        }


        /// <summary>
        /// 获取摄像头状态回调
        /// </summary>
        /// <returns></returns>
        private CameraDevice.StateCallback GetCameraStateCallBack()
        {
            var callBack = new YsCameraCallBack();
            callBack.Act_OnOpened -= CameraStateCallBack_Opened;
            callBack.Act_OnDisconnected -= CameraStateCallBack_Disconnected;
            callBack.Act_OnError -= CameraStateCallBack_Error;

            callBack.Act_OnOpened += CameraStateCallBack_Opened;
            callBack.Act_OnDisconnected += CameraStateCallBack_Disconnected;
            callBack.Act_OnError += CameraStateCallBack_Error;

            return callBack;
        }

        /// <summary>
        /// 摄像头回调_摄像头被打开
        /// </summary>
        /// <param name="camera"></param>
        private void CameraStateCallBack_Opened(CameraDevice camera)
        {

        }

        /// <summary>
        /// 摄像头回调_摄像头被关闭
        /// </summary>
        /// <param name="camera"></param>
        private void CameraStateCallBack_Disconnected(CameraDevice camera)
        {

        }

        /// <summary>
        /// 摄像头回调_摄像头异常
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="error"></param>
        private void CameraStateCallBack_Error(CameraDevice camera, [GeneratedEnum] CameraError error)
        {

        }


        /// <summary>
        /// 获取摄像头预览和拍照请求的Session
        /// </summary>
        /// <returns></returns>
        private CameraCaptureSession.StateCallback GetYsCameraCaptureSessionStateCallback()
        {
            var callBack = new YsCameraCaptureSessionStateCallback();
            callBack.Act_OnConfigured -= CameraCaptureSessionStateCallBack_Configured;
            callBack.Act_OnConfigured += CameraCaptureSessionStateCallBack_Configured;

            callBack.Act_OnConfigureFailed -= CameraCaptureSessionStateCallBack_ConfigureFailed;
            callBack.Act_OnConfigureFailed += CameraCaptureSessionStateCallBack_ConfigureFailed;

            return callBack;
        }

        /// <summary>
        /// 摄像头Session接收
        /// </summary>
        /// <param name="session"></param>
        private void CameraCaptureSessionStateCallBack_Configured(CameraCaptureSession session)
        {
            this.CameraCaptureSession = session;
            this.UpdatePreview();
        }

        /// <summary>
        /// 摄像头Session配置失败
        /// </summary>
        /// <param name="session"></param>
        private void CameraCaptureSessionStateCallBack_ConfigureFailed(CameraCaptureSession session)
        {

        }

        #endregion

        #region 摄像头所需它类
        private class YsCameraCallBack : CameraDevice.StateCallback
        {
            public Action<CameraDevice> Act_OnDisconnected;
            public Action<CameraDevice, CameraError> Act_OnError;
            public Action<CameraDevice> Act_OnOpened;

            public override void OnDisconnected(CameraDevice camera)
            {
                Act_OnDisconnected?.Invoke(camera);
            }

            public override void OnError(CameraDevice camera, [GeneratedEnum] CameraError error)
            {
                Act_OnError?.Invoke(camera, error);
            }

            public override void OnOpened(CameraDevice camera)
            {
                Act_OnOpened?.Invoke(camera);
            }
        }

        private class YsCameraCaptureSessionStateCallback : CameraCaptureSession.StateCallback
        {
            public Action<CameraCaptureSession> Act_OnConfigured;
            public Action<CameraCaptureSession> Act_OnConfigureFailed;

            public override void OnConfigured(CameraCaptureSession session)
            {
                Act_OnConfigured?.Invoke(session);
            }

            public override void OnConfigureFailed(CameraCaptureSession session)
            {
                Act_OnConfigureFailed?.Invoke(session);
            }
        }
        #endregion
    }

}
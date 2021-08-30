using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LibUser.Droid.Tools;
using LibUser.MVVM.Core.ViewModels.MenuContent;

using MvvmCross.Platforms.Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ys.Camera.Droid.Views;
using Ys.TFLite.Core;

namespace LibUser.Droid.Src.Activitys.MenuContent
{
    [Activity(Label = "Acty_CameraX",
        Theme = "@style/Theme.Standard",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Acty_CameraX : MvxActivity<ViewM_CameraX>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Activity_CameraX);

            InitVMParams();
            InitAndStartCameraX();
        }


        private void InitVMParams()
        {
            ViewModel.FilePath_OSRootPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            ViewModel.SetLable2MatList(Tools.ResourcesTools.ReadAssetsInfoForString(this, "Lable2Mat.json"));
            ViewModel.ViewAct_PageClose += delegate
            {
                this.Finish();
            };
            ViewModel.ViewAct_EventTriggerLongClick += delegate
            {
                var intent = new Intent("android.intent.action.GET_CONTENT");
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(intent, 0x123);//打开相册
            };

            ViewModel.EventHandler_Classify += ProcessClassify;
        }

        #region 处理摄像头相关
        private void InitAndStartCameraX()
        {
            var cameraX = FindViewById<YsCameraX>(Resource.Id.cxCameraX);
            cameraX.InitAndStartCamera(this, null);
            //cameraX.CameraInitFinish += delegate
            //{
            //    cameraX.OpenFrameCapture();
            //    cameraX.ImageAnalysisFrameProcess.ImageFrame2NV21ByteCaptured += ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured;
            //};
        }

        private int FlameSkipCount = 0;
        private const int FlameSkipCount_Max = 15;
        private void ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured(object sender, Ys.Camera.Droid.Implements.ImageFrame2Nv21ByteArgs e)
        {
            if (FlameSkipCount < FlameSkipCount_Max)
            {
                FlameSkipCount++;
                return;
            }
            else FlameSkipCount = 0;

            StartTFLiteClassify(e.imgaeNv21Bytes, true);
        }

        #endregion

        #region TFLite识别相关
        private IClassifier defaultClassifier;
        private const string LableName = "tfliteLable.txt";
        private const string ModelName = "tf_lite_model.tflite";
        private void StartTFLiteClassify(byte[] stream, bool isFlameProcess = false)
        {
            if (defaultClassifier == null)
            {
                var stockPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", ModelName);
                defaultClassifier = new TensorflowClassifier(Application.Context.Assets.Open(LableName));
                defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted; ;
            }
            //处理帧数和选择照片的区别
            if (isFlameProcess && ViewModel.LogicLock_AllowFlameClassify)
                ViewModel.ClassifycationStart(stream);
            else if (!isFlameProcess)
                ViewModel.ClassifycationStart(stream);
        }

        private void DefaultClassifier_ClassificationCompleted(object sender, ClassificationEventArgs e)
        {
            var result = e.Predictions.Select(x => new ViewM_CameraX.ClassifyResult
            {
                Percent = x.Probability,
                Tag = x.TagName
            });
            ViewModel.ClassificationCompleted(result.ToList());
        }

        private void ProcessClassify(object sender, byte[] stream)
        {
            Task.Run(async () => { await defaultClassifier.Classify(stream); });
        }
        #endregion

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 0x123)
            {
                var picPath = PathTools.GetRealPathFromUri(this, data.Data);
                var streamByte = ResourcesTools.File2Byte(picPath);
                StartTFLiteClassify(streamByte);
            }

        }

    }
}
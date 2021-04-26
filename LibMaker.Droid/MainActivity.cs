using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Net;
using Android.Views;
using System;
using System.Threading.Tasks;
using AndroidX.Camera.View;
using Ys.Camera.Droid.Views;
using System.Collections.Generic;
using AndroidX.Camera.Core;
using Android.Content;
using System.IO;
using Ys.TFLite.Core;
using System.Linq;
using System.Threading;

namespace LibMaker.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Ys.BeLazy.YsBaseActivity
    {
        private YsCameraX _CameraX;
        private TextView _Info;


        private string SavePicturePath = "";
        private string ModelFilePath = "";

        public override int A_GetContentViewId()
        {
            return Resource.Layout.Activity_Main;
        }

        public override void B_BeforeInitView()
        {
            ModelFilePath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", "converted_model-int8.tflite");
            SavePicturePath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Picture", "pictureOfTakingsss.jpg");
        }

        public override void C_InitView()
        {
            _CameraX = FindViewById<YsCameraX>(Resource.Id.cxCameraX);
            _Info = FindViewById<TextView>(Resource.Id.tvInfo);

            _CameraX.InitAndStartCamera(this);
            _CameraX.CameraInitFinish += delegate
            {
                StartCheckImageFrameThread();
            };

            FindViewById<Button>(Resource.Id.bt_event).Click += delegate (object sender, EventArgs e)
            {
                _CameraX.TakePicture(SavePicturePath, TakePicutreResultHandler_Error, TakePicutreResultHandler_Succsses);
            };

            FindViewById<Button>(Resource.Id.bt_event).LongClick += delegate (object sender, View.LongClickEventArgs ex)
            {
                var intent = new Intent("android.intent.action.GET_CONTENT");
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(intent, 0x123);//打开相册
            };


        }

        public override void D_BindEvent()
        {

        }

        public override void E_InitData()
        {
        }

        public override void F_OnClickListener(View v, EventArgs e)
        {

        }

        private void TakePicutreResultHandler_Succsses(ImageCapture.OutputFileResults outputFileResults)
        {
            RunOnUiThread(() =>
            {
                TFLiteClassifyPorcessStart(SavePicturePath);
            });
        }

        private void TakePicutreResultHandler_Error(ImageCaptureException imageCaptureException)
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(this, "拍照失败", ToastLength.Long).Show();
            });
        }

        #region 线程处理实时处理分类
        private bool isClassifyDone = true;
        private bool isAllow2Classify = false;
        private Thread th_SendClassifyPermission;
        private void StartCheckImageFrameThread()
        {
            if (th_SendClassifyPermission == null)
            {
                th_SendClassifyPermission = new Thread(new ThreadStart(() =>
                {
                    while (true)
                    {
                        if (isClassifyDone)
                            isAllow2Classify = true;
                        else
                            isAllow2Classify = false;
                        Thread.Sleep(2 * 1000);
                    }
                }));
            }
            if (_CameraX != null && _CameraX.ImageAnalysisFrameProcess != null)
            {
                _CameraX.OpenFrameCapture();
                _CameraX.ImageAnalysisFrameProcess.ImageFrame2NV21ByteCaptured += ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured;
                th_SendClassifyPermission.Start();
            }
        }

        private void StopCheckImageFrameThread()
        {
            if (_CameraX != null && _CameraX.ImageAnalysisFrameProcess != null)
            {
                _CameraX.CloseFrameCapture();
                _CameraX.ImageAnalysisFrameProcess.ImageFrame2NV21ByteCaptured -= ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured;
                th_SendClassifyPermission?.Abort();
            }
        }

        private void ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured(object sender, Ys.Camera.Droid.Implements.ImageFrame2Nv21ByteArgs e)
        {
            if (isClassifyDone && isAllow2Classify)
            {
                Task.Factory.StartNew(async () =>
                {
                    isClassifyDone = false;
                    await TFLiteClassifyPorcessStart(e.imgaeNv21Bytes);
                }).ContinueWith(x =>
                {
                    if (x.Exception != null)
                    {
                    }
                }/*, TaskScheduler.FromCurrentSynchronizationContext()*/);
            }
            else
                return;
        }


        #endregion


        private IClassifier defaultClassifier;
        private const string ModelName = "converted_model-int8.tflite";

        private void TFLiteClassifyPorcessStart(string picturePath)
        {
            ShowWaitDialog_Normal("识别中......");

            var streamByte = File2Byte(picturePath);
            if (defaultClassifier == null)
            {
                var stockPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", ModelName);
                defaultClassifier = new TensorflowClassifier(stockPath);
            }
            defaultClassifier.ClassificationCompleted -= DefaultClassifier_ClassificationCompleted;
            defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted;
            Task.Run(async () =>
            {
                await defaultClassifier.Classify(streamByte);
            });
        }

        private async Task TFLiteClassifyPorcessStart(byte[] streamByte)
        {
            if (defaultClassifier == null)
            {
                var stockPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", ModelName);
                defaultClassifier = new TensorflowClassifier(stockPath);
            }
            defaultClassifier.ClassificationCompleted -= DefaultClassifier_ClassificationCompleted;
            defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted;
            await defaultClassifier.Classify(streamByte);
        }


        private void DefaultClassifier_ClassificationCompleted(object sender, ClassificationEventArgs e)
        {
            isClassifyDone = true;
            RunOnUiThread(() =>
            {
                HideWaitDiaLog();
                _Info.Visibility = (e.Predictions != null && e.Predictions.Any()) ? ViewStates.Visible : ViewStates.Visible;
                if (e.Predictions != null && e.Predictions.Any())
                {
                    var orderResult = e.Predictions.OrderByDescending(x => x.Probability).ToList();
                    _Info.Text = $"" +
                    $"识别结果前三为:<{orderResult[0]?.TagName}/{orderResult[1]?.TagName}/{orderResult[2]?.TagName}>" +
                    $"\n识别精度分别为:<{orderResult[0]?.Probability:N2}/{orderResult[1]?.Probability:N2}/{orderResult[2]?.Probability:N2}>";
                }
            });
        }

        private byte[] File2Byte(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                var buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);
                return buffur;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }


        protected override void OnResume()
        {
            base.OnResume();
            StartCheckImageFrameThread();
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopCheckImageFrameThread();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                if (requestCode == 0x123)
                {//选择图片归来
                    var jk = Uri2PathUtil.GetRealPathFromUri(this, data.Data);
                    TFLiteClassifyPorcessStart(jk);
                }

            }

        }


    }
}
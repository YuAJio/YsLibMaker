using Android.App;
using Android.OS;
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
using Yukiho_Threads;

namespace LibMaker.Droid.Src.Activitys
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class Acty_CameraX : Ys.BeLazy.YsBaseActivity
    {
        private YsCameraX _CameraX;
        private TextView _Info;


        private string SavePicturePath = "";
        private string ModelFilePath = "";

        public override int A_GetContentViewId()
        {
            return Resource.Layout.Activity_CameraX;
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

            FindViewById<Button>(Resource.Id.bt_event).Visibility = ViewStates.Gone;

            FindViewById<Button>(Resource.Id.bt_event).Click += delegate (object sender, EventArgs e)
            {
                if (isOpenClassify)
                {
                    SimpTimerPool.Instance.StopTimer("Classify", 0);
                    isOpenClassify = false;
                }
                else
                {
                    SimpTimerPool.Instance.StartTimer("Classify", 0);
                    isOpenClassify = true;
                }
                //_CameraX.TakePicture(SavePicturePath, TakePicutreResultHandler_Error, TakePicutreResultHandler_Succsses);
            };

            FindViewById<Button>(Resource.Id.bt_event).LongClick += delegate (object sender, View.LongClickEventArgs ex)
            {
                //StartActivity(new Intent(this, typeof(Acty_RefreshListView)));
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

        private List<Mat2Lable> listMat2Lable;
        private List<Mat2Lable> ListMat2Lable
        {
            get
            {
                if (listMat2Lable == null)
                {
                    var jsonContent = ReadAssetsInfoForString(this, "Lable2Mat.json");
                    listMat2Lable = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Mat2Lable>>(jsonContent);
                }
                return listMat2Lable;
            }
        }

        /// <summary>
        /// 获取Assets文件中文本文件中数据
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string ReadAssetsInfoForString(Context context, string filePath)
        {
            if (context == null)
                throw new ArgumentException("context不能为空！");

            if (string.IsNullOrWhiteSpace(filePath))
                return null;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                byte[] buffer = new byte[8192];
                int count = 0;
                //获取流
                var stream = context.Assets.Open(filePath, Android.Content.Res.Access.Streaming);
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        //sb.Append(Encoding.ASCII.GetString(buffer, 0, count));
                        sb.Append(System.Text.Encoding.Default.GetString(buffer, 0, count));
                } while (count > 0);

                var content = sb.ToString();
                return content;
            }
            catch (Exception ex)
            {

            }

            return null;
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
        private bool isOpenClassify = true;
        private void StartCheckImageFrameThread()
        {
            SimpTimerPool.Instance.StartAndAddNewLoopTimer("Classify", 0, 2 * 1000, delegate
            {
                if (isClassifyDone)
                    isAllow2Classify = true;
                else
                    isAllow2Classify = false;
            });
            if (_CameraX != null && _CameraX.ImageAnalysisFrameProcess != null)
            {
                _CameraX.OpenFrameCapture();
                _CameraX.ImageAnalysisFrameProcess.ImageFrame2NV21ByteCaptured += ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured;
            }
        }

        private void StopCheckImageFrameThread()
        {
            if (_CameraX != null && _CameraX.ImageAnalysisFrameProcess != null)
            {
                _CameraX.CloseFrameCapture();
                _CameraX.ImageAnalysisFrameProcess.ImageFrame2NV21ByteCaptured -= ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured;
            }
        }

        private int FlameSkipCount = 0;
        private int FlameSkipCount_MAX = 15;
        private void ImageAnalysisFrameProcess_ImageFrame2NV21ByteCaptured(object sender, Ys.Camera.Droid.Implements.ImageFrame2Nv21ByteArgs e)
        {
            if (FlameSkipCount < FlameSkipCount_MAX)
            {
                FlameSkipCount++;
                return;
            }
            else
                FlameSkipCount = 0;
            if (isClassifyDone && isAllow2Classify && isOpenClassify)
            {
                Task.Run(async () =>
                {
                    isClassifyDone = false;
                    await TFLiteClassifyPorcessStart(e.imgaeNv21Bytes);
                }).ContinueWith(x =>
                {
                    if (x.Exception != null)
                    {
                        isClassifyDone = true;
                    }
                }/*, TaskScheduler.FromCurrentSynchronizationContext()*/);
            }
            else
                return;
        }

        #endregion

        private IClassifier defaultClassifier;
        private const string LableName = "tfliteLable.txt";
        private const string ModelName = "converted_model-int8.tflite";
        //private const string ModelName = "converted_model-int8.tflite";

        private void TFLiteClassifyPorcessStart(string picturePath)
        {
            var streamByte = File2Byte(picturePath);
            if (defaultClassifier == null)
            {
                var stockPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", ModelName);
                defaultClassifier = new TensorflowClassifier(stockPath, Application.Context.Assets.Open(LableName));
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
                defaultClassifier = new TensorflowClassifier(stockPath, Application.Context.Assets.Open(LableName));
            }
            defaultClassifier.ClassificationCompleted -= DefaultClassifier_ClassificationCompleted;
            defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted;
            await defaultClassifier.Classify(streamByte);
        }


        private void DefaultClassifier_ClassificationCompleted(object sender, ClassificationEventArgs e)
        {
            isClassifyDone = true;
            string result = "";
            if (e.Predictions != null && e.Predictions.Any())
            {
                //var classifyResult = from j in e.Predictions join k in ListMat2Lable on j.TagName equals k.MatCode select new { j.Probability, k.MatName };

                //var orderResult = classifyResult.OrderByDescending(x => x.Probability).ToList();
                var orderResult =
                    e.Predictions.Select(x => new { x.Probability, MatName = x.TagName })
                    .OrderByDescending(x => x.Probability)
                    .ToList();
                result = $"" +
                $"识别结果前三为:<{orderResult[0]?.MatName}/{orderResult[1]?.MatName}/{orderResult[2]?.MatName}>" +
                $"\n识别精度分别为:<{orderResult[0]?.Probability:N2}/{orderResult[1]?.Probability:N2}/{orderResult[2]?.Probability:N2}>";
            }
            RunOnUiThread(() =>
            {
                _Info.Visibility = string.IsNullOrEmpty(result) ? ViewStates.Gone : ViewStates.Visible;
                _Info.Text = result;
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


        private class Mat2Lable
        {
            public string MatName { get; set; }
            public string MatCode { get; set; }
        }


    }
}
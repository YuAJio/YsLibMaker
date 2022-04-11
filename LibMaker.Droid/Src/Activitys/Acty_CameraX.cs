using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Camera.Core;

using LibMaker.Droid.Src.Manager;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Ys.Camera.Droid.Views;

using Yukiho_Threads;

namespace LibMaker.Droid.Src.Activitys
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class Acty_CameraX : Ys.BeLazy.Base.YsBaseActivity
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

            _CameraX.InitAndStartCamera(this, (bo, str) =>
            {
                if (bo)
                {
                    StartCheckImageFrameThread();
                    InitTFLiteManager();
                }
            });
            //_CameraX.CameraInitFinish += delegate
            //{
            //    StartCheckImageFrameThread();
            //};

            FindViewById<Button>(Resource.Id.bt_event).Click += delegate (object sender, EventArgs e)
            {
                ShowWaitDialog_Samll("少女祈祷中");
                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(2 * 1000);
                    RunOnUiThread(() =>
                    {
                        StartActivity(new Intent(this, typeof(Acty_RefreshListView)));
                        this.Finish();
                    });
                    Thread.Sleep(2 * 1000);
                    RunOnUiThread(() =>
                    {
                        HideWaitDiaLog();
                    });
                })).Start();
                //if (isOpenClassify)
                //{
                //    SimpTimerPool.Instance.StopTimer("Classify", 0);
                //    isOpenClassify = false;
                //}
                //else
                //{
                //    SimpTimerPool.Instance.StartTimer("Classify", 0);
                //    isOpenClassify = true;
                //}
                //_CameraX.TakePicture(SavePicturePath, TakePicutreResultHandler_Error, TakePicutreResultHandler_Succsses);
            };


            return;
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
            //RunOnUiThread(() =>
            //{
            //    TFLiteClassifyPorcessStart(SavePicturePath);
            //});
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
            FlameSkipCount = 0;

            ysTFLiteMag?.Classify(e.imgaeNv21Bytes);
        }

        #endregion

        private const string ModelName = "model.tflite";
        private const string LableName = "tfliteLable.txt";
        private YsMatClassify_TFLiteMag ysTFLiteMag;
        private void InitTFLiteManager()
        {
            if (ysTFLiteMag == null)
            {
                string rootPath;
                if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                    rootPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                else
                    rootPath = Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath;

                var lablePath = System.IO.Path.Combine(rootPath, "TFLite", "Lable", LableName);
                var modelPath = System.IO.Path.Combine(rootPath, "TFLite", "Model", ModelName);
                var lableNameJson = ReadAssetsInfoForString(this, "Lable2Mat.json");
                ysTFLiteMag = new YsMatClassify_TFLiteMag(lablePath, modelPath);
                ysTFLiteMag.ErrorCallBack += YsTFLiteMag_ErrorCallBack;
                ysTFLiteMag.ClassifyCompleteEvent += YsTFLiteMag_ClassifyCompleteEvent;
                ysTFLiteMag.TFliteClassifyInit();
                ysTFLiteMag.SetLableCode(lableNameJson);
            }
        }

        private void YsTFLiteMag_ClassifyCompleteEvent(object sender, List<YsMatClassify_TFLiteMag.ResultObj> e)
        {
            if (e.Any())
            {
                var txt = "";
                if (e.Count >= 3)
                    txt = $"{e[0].Name}:{e[0].Probability:N2}\t{e[1].Name}:{e[1].Probability:N2}\t{e[2].Name}:{e[2].Probability:N2}";
                else if (e.Count >= 2)
                    txt = $"{e[0].Name}:{e[0].Probability:N2}\t{e[1].Name}:{e[1].Probability:N2}";
                else if (e.Count >= 1)
                    txt = $"{e[0].Name}:{e[0].Probability:N2}";

                RunOnUiThread(() =>
                {
                    _Info.Visibility = ViewStates.Visible;
                    _Info.Text = txt;
                });
            }
        }

        private void YsTFLiteMag_ErrorCallBack(string arg1, Exception ex)
        {
            RunOnUiThread(() =>
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).Show();
            });

            isAllow2Classify = false;
            SimpTimerPool.Instance.StopTimer("Classify", 0);
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

                    var stockPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model");
                    var root = new DirectoryInfo(stockPath);
                    var filesName = root.GetFiles().Select(x => x.Name).ToArray();

                    ysTFLiteMag.Classify(File2Byte(jk));
                    //ShowSingleChoseDialog(filesName, (j) =>
                    //{
                    //    //ModelName = filesName[j];
                    //    //TFLiteClassifyPorcessStart(jk);
                    //});

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
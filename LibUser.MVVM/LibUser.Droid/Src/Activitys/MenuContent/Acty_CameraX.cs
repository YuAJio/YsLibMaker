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
        }


        private void InitVMParams()
        {
            ViewModel.FilePath_OSRootPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            ViewModel.SetLable2MatList(Tools.ResourcesTools.ReadAssetsInfoForString(this, "Lable2Mat"));
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
        }

        #region 处理摄像头相关



        #endregion

        #region TFLite识别相关
        private IClassifier defaultClassifier;
        private const string LableName = "tfliteLable.txt";
        private const string ModelName = "tf_lite_model.tflite";
        private void StartTFLiteClassify(byte[] stream)
        {
            ViewModel.EventHandler_Classify -= ProcessClassify;
            ViewModel.EventHandler_Classify += ProcessClassify;

            if (defaultClassifier == null)
            {
                var stockPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TFLite", "Model", ModelName);
                defaultClassifier = new TensorflowClassifier(stockPath, Application.Context.Assets.Open(LableName));
                defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted; ;
            }
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
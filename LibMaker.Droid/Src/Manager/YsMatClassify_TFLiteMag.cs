using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Ys.TFLite.Core;


namespace LibMaker.Droid.Src.Manager
{
    public class YsMatClassify_TFLiteMag
    {
        private string LableFilePaht;
        private string ModelFilePath;
        public YsMatClassify_TFLiteMag(string lableFilePath, string modelFilePath)
        {
            this.LableFilePaht = lableFilePath;
            this.ModelFilePath = modelFilePath;
        }
        #region 事件申明
        public event Action<string, Exception> ErrorCallBack;
        public event EventHandler<List<ResultObj>> ClassifyCompleteEvent;
        #endregion


        private IClassifier defaultClassifier;
        private bool isClassifyDone = true;

        public void TFliteClassifyInit()
        {
            if (defaultClassifier == null)
            {
                if (!File.Exists(LableFilePaht) || !File.Exists(ModelFilePath))
                {
                    ErrorCallBack?.Invoke($"{(!File.Exists(ModelFilePath) ? "模型" : "标签")}文件未找到,打开分类引擎失败", new ArgumentException());
                    return;
                }
                defaultClassifier = new TensorflowClassifier(File.OpenRead(LableFilePaht));
                defaultClassifier.SetTFLiteModelPath(ModelFilePath);
                defaultClassifier.ClassificationCompleted -= DefaultClassifier_ClassificationCompleted;
                defaultClassifier.ClassificationCompleted += DefaultClassifier_ClassificationCompleted;
            }
        }

        public void Classify(byte[] nv21Stream)
        {
            if (!isClassifyDone)
                return;
            isClassifyDone = false;
            //new System.Threading.Thread(async () =>
            //{
            try
            {
                defaultClassifier?.Classify(nv21Stream);
            }
            catch (Exception e)
            {
                isClassifyDone = true;
                ErrorCallBack?.Invoke("分类流程出现异常", e);
            }
            //}).Start();
        }

        private void DefaultClassifier_ClassificationCompleted(object sender, ClassificationEventArgs e)
        {
            isClassifyDone = true;
            var content = new List<ResultObj>();
            if (e.Predictions != null && e.Predictions.Any())
            {
                var classifyResult = from j in e.Predictions
                                     join k in ListMat2Lable on j.TagName equals k.MatCode
                                     select new
                                     {
                                         j.Probability,
                                         k.MatName,
                                     };
                var orderResult =
                   classifyResult.OrderByDescending(x => x.Probability)
                    .Take(3)
                    .Select(x => new ResultObj { Name = x.MatName, Probability = x.Probability });
                content.AddRange(orderResult);
            }
            ClassifyCompleteEvent?.Invoke(this, content);
        }

        private List<Code2Name> ListMat2Lable;
        public void SetLableCode(string jsonData)
        {
            ListMat2Lable = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Code2Name>>(jsonData);
        }
        public bool IsClassifing()
        {
            return !isClassifyDone;
        }


        #region 相关实体类
        private class Code2Name
        {
            public string MatCode { get; set; }
            public string MatName { get; set; }
        }

        public class ResultObj
        {
            public string Name { get; set; }
            public float Probability { get; set; }
        }
        #endregion


    }
}
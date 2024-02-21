using NETCore.Encrypt;

using NumSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.TensorFlow.Lite;

using Ys.TFLite.Core.Models;

namespace Ys.TFLite.Core
{
    public interface IClassifier
    {
        event EventHandler<ClassificationEventArgs> ClassificationCompleted;
        void Classify(byte[] bytes);
        string GetTFLiteModelPath();
        void SetTFLiteModelPath(string modelPath);
    }
    public class ClassificationEventArgs : EventArgs
    {
        public List<Classification> Predictions { get; private set; }

        public ClassificationEventArgs(List<Classification> predictions)
        {
            Predictions = predictions;
        }
    }

    public class TensorflowClassifier : IClassifier
    {
        public const int FloatSize = 4;
        public const int PixelSize = 3;

        private string ModelPath;

        private Interpreter interpreter;
        private ITensor tensor;
        private List<string> List_Labels;
        private bool isClassifying = false;
        private bool isInitingModel = false;

        public TensorflowClassifier(Stream LabelStream)
        {
            List_Labels = new StreamReader(LabelStream).ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        public event EventHandler<ClassificationEventArgs> ClassificationCompleted;

        public void InitModel()
        {
            if (isInitingModel)
                return;
            isInitingModel = true;
            if (!System.IO.File.Exists(ModelPath))
                return;
            interpreter = new Interpreter(new Java.IO.File(ModelPath));

            tensor ??= interpreter.GetInputTensor(0);

            isInitingModel = false;
        }
        public void Classify(byte[] bytes)
        {
            if (interpreter == null || tensor == null)
            {
                InitModel();
                return;
            }
            if (isClassifying)
                return;
            isClassifying = true;
            new Thread(new ThreadStart(async () =>
           {
               var shape = tensor.Shape();
               var width = shape[1];
               var height = shape[2];

               var byteBuffer = await GetByteBufferFromPhoto(bytes, width, height);
               if (byteBuffer == null)
               {
                   ClassificationCompleted?.Invoke(this, new ClassificationEventArgs(new List<Classification>()));
                   return;
               }
               if (List_Labels == null)
               {
                   ClassificationCompleted?.Invoke(this, new ClassificationEventArgs(new List<Classification>()));
                   return;
               }
               var outputLocations = new float[1][] { new float[List_Labels.Count] };
               var outputs = Java.Lang.Object.FromArray(outputLocations);
               interpreter.Run(byteBuffer, outputs);
               var classificationResult = outputs.ToArray<float[]>();
               var result = new List<Classification>();
               for (var i = 0; i < List_Labels.Count; i++)
               {
                   var label = List_Labels[i];
                   result.Add(new Classification { TagName = label, Probability = classificationResult[0][i] });
               }
               isClassifying = false;
               ClassificationCompleted?.Invoke(this, new ClassificationEventArgs(result));
           })).Start();
        }

        public string GetTFLiteModelPath()
        {
            return ModelPath;
        }
        public void SetTFLiteModelPath(string modelPath)
        {
            this.ModelPath = modelPath;
        }

        private async Task<Java.Nio.ByteBuffer> GetByteBufferFromPhoto(byte[] bytes, int width, int height)
        {
            var modelInputSize = FloatSize * height * width * PixelSize;

            var bitmap = await Android.Graphics.BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length);
            if (bitmap == null)
                return null;
            var resizedBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, width, height, true);
            if (resizedBitmap == null)
                return null;

            var byteBuffer = Java.Nio.ByteBuffer.AllocateDirect(modelInputSize);
            byteBuffer.Order(Java.Nio.ByteOrder.NativeOrder());

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;
            var jkBytre = new byte[width * height * 3 * 4];
            var jkpixel = 0;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];
                    foreach (var item_m in new float[] {
                      pixelVal >> 16 & 0xFF,
                      pixelVal >> 8 & 0xFF,
                       pixelVal >> 0 & 0xFF})
                    {
                        var item_procs = ((item_m / 255f) - 0.5f) * 2.0f;
                        foreach (var item_s in BitConverter.GetBytes(item_procs))
                        {
                            jkBytre[jkpixel] = item_s;
                            jkpixel++;
                        }
                    }
                }
            }

            byteBuffer.Put(jkBytre, 0, jkBytre.Length);

            bitmap.Recycle();
            return byteBuffer;
        }

        #region 模型文件解密
        //private string GetDecryPtModelFile(string filePath)
        //{
        //    var aseKey = EncryptProvider.CreateAesKey();
        //    aseKey.Key = "SAGjstcSTC79dx2SwsYfS23xeUHKBgLq";
        //    aseKey.IV = "0SyzJl5yaZqwIWj5";
        //    var b2 = File.ReadAllBytes(filePath);
        //    var dBytes = EncryptProvider.AESDecrypt(b2, aseKey.Key, aseKey.IV);
        //    var deFilePath = filePath + "_DE";
        //    File.WriteAllBytesAsync(deFilePath, dBytes).GetAwaiter().GetResult();
        //    return deFilePath;
        //}
        #endregion

    }


}
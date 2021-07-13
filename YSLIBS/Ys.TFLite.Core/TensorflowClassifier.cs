using NETCore.Encrypt;

using NumSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.TensorFlow.Lite;

using Ys.TFLite.Core.Models;

namespace Ys.TFLite.Core
{
    public interface IClassifier
    {
        event EventHandler<ClassificationEventArgs> ClassificationCompleted;
        Task Classify(byte[] bytes);
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
        private Tensor tensor;
        private List<string> List_Lables;

        public TensorflowClassifier(Stream lableStream)
        {
            List_Lables = new StreamReader(lableStream).ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); ;
        }

        public event EventHandler<ClassificationEventArgs> ClassificationCompleted;

        public async Task Classify(byte[] bytes)
        {
            try
            {
                if (!System.IO.File.Exists(ModelPath))
                    return;
                var deModelPath = GetDecryPtModelFile(ModelPath);
                if (!File.Exists(deModelPath))
                    return;
                if (interpreter == null)
                {
                    interpreter = new Interpreter(new Java.IO.File(ModelPath));
                    File.Delete(deModelPath);
                }

                if (tensor == null)
                    tensor = interpreter.GetInputTensor(0);

                var shape = tensor.Shape();
                var width = shape[1];
                var height = shape[2];

                var byteBuffer = await GetByteBufferFromPhoto(bytes, width, height);
                var outputLocations = new float[1][] { new float[List_Lables.Count] };
                var outputs = Java.Lang.Object.FromArray(outputLocations);
                interpreter.Run(byteBuffer, outputs);
                var classificationResult = outputs.ToArray<float[]>();
                var result = new List<Classification>();
                for (var i = 0; i < List_Lables.Count; i++)
                {
                    var label = List_Lables[i];
                    result.Add(new Classification { TagName = label, Probability = classificationResult[0][i] });
                }
                ClassificationCompleted?.Invoke(this, new ClassificationEventArgs(result));
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
            var resizedBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, width, height, true);

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
        private string GetDecryPtModelFile(string filePath)
        {
            var aseKey = EncryptProvider.CreateAesKey();
            aseKey.Key = "SAGjstcSTC79dx2SwsYfS23xeUHKBgLq";
            aseKey.IV = "0SyzJl5yaZqwIWj5";
            var b2 = File.ReadAllBytes(filePath);
            var dBytes = EncryptProvider.AESDecrypt(b2, aseKey.Key, aseKey.IV);
            var deFilePath = filePath + "_DE";
            File.WriteAllBytesAsync(deFilePath, dBytes).GetAwaiter().GetResult();
            return deFilePath;
        }
        #endregion

    }


}
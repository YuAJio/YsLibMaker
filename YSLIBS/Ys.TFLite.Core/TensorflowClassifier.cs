﻿using System;
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

        private readonly string ModelPath;

        private Interpreter interpreter;
        private Tensor tensor;
        private List<string> List_Lables;

        public TensorflowClassifier(string modelPath, Stream lableStream)
        {
            ModelPath = modelPath;
            List_Lables = new StreamReader(lableStream).ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); ;
        }

        public event EventHandler<ClassificationEventArgs> ClassificationCompleted;

        public async Task Classify(byte[] bytes)
        {
            try
            {
                if (!System.IO.File.Exists(ModelPath))
                    return;
                if (interpreter == null)
                    interpreter = new Interpreter(new Java.IO.File(ModelPath));
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

        public static float[] TORCHVISION_NORM_MEAN_RGB = new float[] { 0.485f, 0.456f, 0.406f };
        public static float[] TORCHVISION_NORM_STD_RGB = new float[] { 0.229f, 0.224f, 0.225f };
        /**
         *    (( ( (pixelVal >> 16) & 0xFF)/255.0f )-TORCHVISION_NORM_MEAN_RGB[0])/TORCHVISION_NORM_STD_RGB[0],
               (( ((pixelVal >> 8) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[1])/TORCHVISION_NORM_STD_RGB[1],
               ((((pixelVal >> 0) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[2])/TORCHVISION_NORM_STD_RGB[2] })
         * */


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
               //         (( ( (pixelVal >> 16) & 0xFF)/255.0f )-TORCHVISION_NORM_MEAN_RGB[0])/TORCHVISION_NORM_STD_RGB[0],
               //(( ((pixelVal >> 8) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[1])/TORCHVISION_NORM_STD_RGB[1],
               //((((pixelVal >> 0) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[2])/TORCHVISION_NORM_STD_RGB[2] })
                    {
                        foreach (var item_s in BitConverter.GetBytes(item_m))
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

        private async Task<Java.Nio.FloatBuffer> GetFloatBufferFromPhoto(byte[] bytes, int width, int height)
        {
            var modelInputSize = FloatSize * height * width * PixelSize;

            var bitmap = await Android.Graphics.BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length);
            var resizedBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, width, height, true);

            var byteBuffer = Java.Nio.FloatBuffer.Allocate(modelInputSize);
            byteBuffer.Order();

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;
            var jkBytre = new float[width * height * 3 * 4];
            var jkpixel = 0;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];
                    foreach (var item_m in new float[] {
                      (( ( (pixelVal >> 16) & 0xFF)/255.0f )-TORCHVISION_NORM_MEAN_RGB[0])/TORCHVISION_NORM_STD_RGB[0],
                       (( ((pixelVal >> 8) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[1])/TORCHVISION_NORM_STD_RGB[1],
                        ((((pixelVal >> 0) & 0xFF)/255.0f)-TORCHVISION_NORM_MEAN_RGB[2])/TORCHVISION_NORM_STD_RGB[2] })
                    {
                        jkBytre[jkpixel] = item_m;
                        jkpixel++;
                        //foreach (var item_s in item_m)
                        //{
                        //    jkBytre[jkpixel] = item_s;
                        //    jkpixel++;
                        //}
                    }
                }
            }

            byteBuffer.Put(jkBytre, 0, jkBytre.Length);

            bitmap.Recycle();
            return byteBuffer;
        }


    }


}
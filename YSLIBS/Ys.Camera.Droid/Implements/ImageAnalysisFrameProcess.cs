using Android.Graphics;
using AndroidX.Camera.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Ys.Camera.Droid.Implements
{
    public class ImageAnalysisFrameProcess : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        //public event EventHandler<ImageFrameArgs> ImageFrameCaptured;
        public event EventHandler<ImageFrame2RgbBytesArgs> RgbBytesCali;

        //public bool IsOpenFrameCapture { get { return isOpenFrameCapture; } set { isOpenFrameCapture = value; } }
        //private bool isOpenFrameCapture = true;

        public bool EnableAIDetect { get { return enableAIDetect; } set { enableAIDetect = value; } }
        private bool enableAIDetect = false;

        private CancellationTokenSource saveBitmapCts = new CancellationTokenSource();
        private int savedImageCount = 0;
        private const int MaxSavedImages = 3;
        private const int SaveIntervalMs = 5000; // 5秒
        private const string SaveDirectory = "/storage/emulated/0/Download/"; // 目标保存目录，可调整

        public ImageAnalysisFrameProcess()
        {
            //// 确保保存目录存在
            //try
            //{
            //    if (!Directory.Exists(SaveDirectory))
            //    {
            //        Directory.CreateDirectory(SaveDirectory);
            //    }
            //    StartPeriodicSaveTask();
            //}
            //catch (Exception ex)
            //{
            //}
        }

        public void StopAnalyzer()
        {
            //isOpenFrameCapture = false;
            enableAIDetect = false;
        }

        public void Analyze(IImageProxy image)
        {
            try
            {
                //if (IsOpenFrameCapture)
                //    ImageFrameCaptured?.Invoke(this, new ImageFrameArgs(image));

                if (EnableAIDetect)
                {
                    var width = image.Width;
                    var height = image.Height;
                    var rgbBytes = ConvertToRgbByteArray(image);
                    // 验证RGB字节数组
                    bool isValid = ValidateRgbBytes(rgbBytes, width, height);
                    RgbBytesCali?.Invoke(this, new ImageFrame2RgbBytesArgs(rgbBytes, width, height, isValid));
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                image.Close();
            }
        }

        #region NV21_RGB
        /// <summary>
        /// 将 CameraX 的 ImageProxy (YUV_420_888 格式) 转换为 RGB 格式的 byte[] 数组。
        /// </summary>
        /// <param name="image">CameraX 的 ImageProxy 对象</param>
        /// <returns>RGB 格式的 byte[] 数组，每个像素占用 3 个字节（R、G、B）</returns>
        /// <exception cref="ArgumentException">如果图像格式不是 YUV_420_888，则抛出异常</exception>
        public static byte[] ConvertToRgbByteArray(IImageProxy image)
        {
            if (image.Format != (int)ImageFormatType.Yuv420888)
            {
                throw new ArgumentException("仅支持 YUV_420_888 格式");
            }

            int width = image.Width;
            int height = image.Height;

            var planes = image.GetPlanes();
            var yPlane = planes[0]; // 亮度平面
            var uPlane = planes[1]; // 蓝色色度平面 (Cb)
            var vPlane = planes[2]; // 红色色度平面 (Cr)

            var yBuffer = yPlane.Buffer;
            var uBuffer = uPlane.Buffer;
            var vBuffer = vPlane.Buffer;

            byte[] yBytes = new byte[yBuffer.Remaining()];
            yBuffer.Get(yBytes);

            byte[] uBytes = new byte[uBuffer.Remaining()];
            uBuffer.Get(uBytes);

            byte[] vBytes = new byte[vBuffer.Remaining()];
            vBuffer.Get(vBytes);

            int yPixelStride = yPlane.PixelStride;
            int yRowStride = yPlane.RowStride;
            int uPixelStride = uPlane.PixelStride;
            int uRowStride = uPlane.RowStride;
            int vPixelStride = vPlane.PixelStride;
            int vRowStride = vPlane.RowStride;

            byte[] rgbArray = new byte[width * height * 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int yIndex = y * yRowStride + x * yPixelStride;
                    int uIndex = (y / 2) * uRowStride + (x / 2) * uPixelStride;
                    int vIndex = (y / 2) * vRowStride + (x / 2) * vPixelStride;

                    // 获取 Y、U、V 值，YUV值可能为负，需转换为无符号值
                    float Y = (yBytes[yIndex] & 0xFF); // 转换为无符号值 (0-255)
                    float U = (uBytes[uIndex] & 0xFF) - 128; // 转换为无符号后偏移
                    float V = (vBytes[vIndex] & 0xFF) - 128; // 转换为无符号后偏移

                    // YUV 到 RGB 转换公式（基于ITU-R BT.601标准）
                    float R = Y + 1.402f * V;
                    float G = Y - 0.344136f * U - 0.714136f * V;
                    float B = Y + 1.772f * U;

                    // 限制 RGB 值在 0-255 范围内
                    int r = (int)Math.Max(0, Math.Min(255, R));
                    int g = (int)Math.Max(0, Math.Min(255, G));
                    int b = (int)Math.Max(0, Math.Min(255, B));

                    int index = (y * width + x) * 3;
                    rgbArray[index] = (byte)r;
                    rgbArray[index + 1] = (byte)g;
                    rgbArray[index + 2] = (byte)b;
                }
            }

            return rgbArray;
        }

        /// <summary>
        /// 验证 RGB 字节数组是否有效，适合 AI 识别
        /// </summary>
        /// <param name="rgbBytes">RGB 字节数组</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>如果有效返回 true，否则返回 false</returns>
        private bool ValidateRgbBytes(byte[] rgbBytes, int width, int height)
        {
            try
            {
                // 检查数组长度是否正确
                if (rgbBytes == null || rgbBytes.Length != width * height * 3)
                {
                    return false;
                }

                // 检查是否有有效的颜色值（非全黑或非全白）
                int nonZeroCount = 0;
                int sampleSize = Math.Min(rgbBytes.Length / 3, 1000); // 采样检查，最多1000个像素
                for (int i = 0; i < sampleSize; i++)
                {
                    int index = i * 3;
                    byte r = rgbBytes[index];
                    byte g = rgbBytes[index + 1];
                    byte b = rgbBytes[index + 2];
                    // 如果像素不是全0（黑）或全255（白），认为有颜色
                    if (r != 0 || g != 0 || b != 0)
                    {
                        nonZeroCount++;
                    }
                    if (r != 255 || g != 255 || b != 255)
                    {
                        nonZeroCount++;
                    }
                }

                // 如果采样中有效颜色比例过低（<10%），可能图像无效
                bool isValid = nonZeroCount > sampleSize * 0.1;
                return isValid;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 将 RGB 字节数组转换为 Bitmap 并保存到指定目录
        /// </summary>
        /// <param name="rgbBytes">RGB 字节数组</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="filePath">保存路径</param>
        /// <returns>是否成功保存</returns>
        private bool ConvertAndSaveToBitmap(byte[] rgbBytes, int width, int height, string filePath)
        {
            try
            {
                // 创建 Bitmap
                Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                int[] pixels = new int[width * height];
                for (int i = 0; i < width * height; i++)
                {
                    int index = i * 3;
                    int r = rgbBytes[index] & 0xFF;
                    int g = rgbBytes[index + 1] & 0xFF;
                    int b = rgbBytes[index + 2] & 0xFF;
                    // ARGB_8888 格式：Alpha=255（不透明），R、G、B
                    pixels[i] = unchecked((int)(0xFF000000 | (r << 16) | (g << 8) | b));
                }
                bitmap.SetPixels(pixels, 0, width, 0, 0, width, height);

                // 保存到文件
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                }
                bitmap.Recycle();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        ///// <summary>
        ///// 启动周期性保存任务，每5秒保存一张，最多3张
        ///// </summary>
        //private void StartPeriodicSaveTask()
        //{
        //    Task.Run(async () =>
        //    {
        //        while (!saveBitmapCts.Token.IsCancellationRequested)
        //        {
        //            if (savedImageCount >= MaxSavedImages)
        //            {
        //                break;
        //            }

        //            //if (EnableAIDetect)
        //            //{
        //            // 等待下一次 RgbBytesCali 事件获取 RGB 数据
        //            var tcs = new TaskCompletionSource<ImageFrame2RgbBytesArgs>();
        //            EventHandler<ImageFrame2RgbBytesArgs> handler = null;
        //            handler = (sender, args) =>
        //            {
        //                RgbBytesCali -= handler; // 移除事件监听
        //                tcs.TrySetResult(args);
        //            };
        //            RgbBytesCali += handler;

        //            try
        //            {
        //                var args = await tcs.Task;
        //                if (args.rgbBytes != null && args.isValid)
        //                {
        //                    string filePath = Path.Combine(SaveDirectory,
        //                        $"Frame_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        //                    if (ConvertAndSaveToBitmap(args.rgbBytes, args.width, args.height, filePath))
        //                    {
        //                        savedImageCount++;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //            //}
        //            await Task.Delay(SaveIntervalMs, saveBitmapCts.Token);
        //        }
        //    }, saveBitmapCts.Token);
        //}

        ///// <summary>
        ///// 停止保存任务
        ///// </summary>
        //public void StopSaving()
        //{
        //    saveBitmapCts?.Cancel();
        //    saveBitmapCts = new CancellationTokenSource();
        //}
    }

    public class ImageFrameArgs : EventArgs
    {
        public IImageProxy imageProxy;

        public ImageFrameArgs(IImageProxy imageProxy)
        {
            this.imageProxy = imageProxy;
        }
    }

    public class ImageFrame2RgbBytesArgs : EventArgs
    {
        public byte[] rgbBytes;
        public int width;
        public int height;
        public bool isValid; // 新增：指示RGB数据是否有效

        public ImageFrame2RgbBytesArgs(byte[] rgbBytes, int width, int height, bool isValid = true)
        {
            this.rgbBytes = rgbBytes;
            this.width = width;
            this.height = height;
            this.isValid = isValid;
        }
    }
}
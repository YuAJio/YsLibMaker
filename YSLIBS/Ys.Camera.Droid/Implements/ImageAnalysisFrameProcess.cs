using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Camera.Core;
using AndroidX.Camera.Core.Internal.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ys.Camera.Droid.Implements
{
    public class ImageAnalysisFrameProcess : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        public event EventHandler<ImageFrameArgs> ImageFrameCaptured;

        public event EventHandler<ImageFrame2RgbBytesArgs> RgbBytesCali;

        public bool IsOpenFrameCapture { get { return isOpenFrameCapture; } set { isOpenFrameCapture = value; } }
        private bool isOpenFrameCapture = true;

        public bool EnableAIDetect { get { return enableAIDetect; } set { enableAIDetect = value; } }
        private bool enableAIDetect = false;

        public void Analyze(IImageProxy image)
        {
            try
            {
                if (IsOpenFrameCapture)
                    ImageFrameCaptured?.Invoke(this, new ImageFrameArgs(image));

                if (EnableAIDetect)
                {
                    var width = image.Width;
                    var height = image.Height;
                    var rgbBytes = ConvertToRgbByteArray(image);
                    RgbBytesCali?.Invoke(this, new ImageFrame2RgbBytesArgs(rgbBytes, width, height));
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
            // 检查图像格式
            if (image.Format != (int)ImageFormatType.Yuv420888)
            {
                throw new ArgumentException("仅支持 YUV_420_888 格式");
            }

            // 获取图像宽度和高度
            int width = image.Width;
            int height = image.Height;

            // 获取 Y、U、V 平面
            var planes = image.GetPlanes();
            var yPlane = planes[0]; // 亮度平面
            var uPlane = planes[1]; // 蓝色色度平面 (Cb)
            var vPlane = planes[2]; // 红色色度平面 (Cr)

            // 获取每个平面的 ByteBuffer
            var yBuffer = yPlane.Buffer;
            var uBuffer = uPlane.Buffer;
            var vBuffer = vPlane.Buffer;

            // 将 ByteBuffer 数据复制到 byte[] 数组
            byte[] yBytes = new byte[yBuffer.Remaining()];
            yBuffer.Get(yBytes);

            byte[] uBytes = new byte[uBuffer.Remaining()];
            uBuffer.Get(uBytes);

            byte[] vBytes = new byte[vBuffer.Remaining()];
            vBuffer.Get(vBytes);

            // 获取每个平面的 pixelStride 和 rowStride
            int yPixelStride = yPlane.PixelStride;
            int yRowStride = yPlane.RowStride;
            int uPixelStride = uPlane.PixelStride;
            int uRowStride = uPlane.RowStride;
            int vPixelStride = vPlane.PixelStride;
            int vRowStride = vPlane.RowStride;

            // 初始化 RGB byte[] 数组
            byte[] rgbArray = new byte[width * height * 3];

            // 遍历每个像素，转换为 RGB
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 计算 Y、U、V 在 byte[] 中的索引
                    int yIndex = y * yRowStride + x * yPixelStride;
                    int uIndex = (y / 2) * uRowStride + (x / 2) * uPixelStride;
                    int vIndex = (y / 2) * vRowStride + (x / 2) * vPixelStride;

                    // 获取 Y、U、V 值
                    float Y = yBytes[yIndex];         // 亮度值 (0-255)
                    float U = uBytes[uIndex] - 128;   // 蓝色色度偏移
                    float V = vBytes[vIndex] - 128;   // 红色色度偏移

                    // YUV 到 RGB 转换公式
                    int R = (int)(Y + 1.402f * V);
                    int G = (int)(Y - 0.344136f * U - 0.714136f * V);
                    int B = (int)(Y + 1.772f * U);

                    // 将 RGB 值限制在 0-255 范围内
                    R = Math.Max(0, Math.Min(255, R));
                    G = Math.Max(0, Math.Min(255, G));
                    B = Math.Max(0, Math.Min(255, B));

                    // 将 RGB 值存储到 byte[] 数组
                    int index = (y * width + x) * 3;
                    rgbArray[index] = (byte)R;
                    rgbArray[index + 1] = (byte)G;
                    rgbArray[index + 2] = (byte)B;
                }
            }

            return rgbArray;
        }
        #endregion
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

        public ImageFrame2RgbBytesArgs(byte[] rgbBytes, int width, int height)
        {
            this.rgbBytes = rgbBytes;
            this.width = width;
            this.height = height;
        }
    }

}
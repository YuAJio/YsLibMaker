using Android.App;
using Android.Content;
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

        public event EventHandler<ImageFrame2Nv21ByteArgs> ImageFrame2NV21ByteCaptured;

        public bool IsOpenFrameCapture { get { return isOpenFrameCapture; } set { isOpenFrameCapture = value; } }
        private bool isOpenFrameCapture = true;

        public void Analyze(IImageProxy image)
        {
            try
            {
                //Console.WriteLine($"Yurishi Test, analyzeWidth:{image.Width}, analyzeHeight:{image.Height}");
                if (IsOpenFrameCapture)
                {
                    ImageFrameCaptured?.Invoke(this, new ImageFrameArgs(image));
                    var byteData = ImageUtil.YuvImageToJpegByteArray(image, null, 80, 180);
                    var jk = new ImageFrame2Nv21ByteArgs(byteData);
                    ImageFrame2NV21ByteCaptured?.Invoke(this, jk);
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
    }

    public class ImageFrameArgs : EventArgs
    {
        public IImageProxy imageProxy;

        public ImageFrameArgs(IImageProxy imageProxy)
        {
            this.imageProxy = imageProxy;
        }
    }

    public class ImageFrame2Nv21ByteArgs : EventArgs
    {
        public byte[] imgaeNv21Bytes;

        public ImageFrame2Nv21ByteArgs(byte[] imgaeNv21Bytes)
        {
            this.imgaeNv21Bytes = imgaeNv21Bytes;
        }
    }

}
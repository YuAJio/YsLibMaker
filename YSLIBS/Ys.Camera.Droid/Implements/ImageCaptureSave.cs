using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ys.Camera.Droid.Implements
{
    public class ImageCaptureSave : Java.Lang.Object, ImageCapture.IOnImageSavedCallback
    {
        public Action<ImageCaptureException> Act_OnError { get; set; }
        public Action<ImageCapture.OutputFileResults> Act_OnImageSaved { get; set; }

        public ImageCaptureSave(Action<ImageCaptureException> act_OnError, Action<ImageCapture.OutputFileResults> act_OnImageSaved)
        {
            Act_OnError = act_OnError;
            Act_OnImageSaved = act_OnImageSaved;
        }

        public void OnError(ImageCaptureException exception)
        {
            Act_OnError?.Invoke(exception);
        }

        public void OnImageSaved(ImageCapture.OutputFileResults outputFileResults)
        {
            Act_OnImageSaved?.Invoke(outputFileResults);
        }

    }
}
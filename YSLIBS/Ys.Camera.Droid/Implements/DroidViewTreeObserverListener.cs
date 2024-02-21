using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ys.Camera.Droid.Implements
{
    public class DroidViewTreeObserverListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        public DroidViewTreeObserverListener(Action act, View v)
        {
            this.GlobalLayoutAct = act;
            this.v = v;
        }
        public event Action GlobalLayoutAct = null;
        private View v;
        public void OnGlobalLayout()
        {
            GlobalLayoutAct?.Invoke();
            v.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Ys.BeLazy.Views
{
    public class Ys_SlidingPaneLayout : SlidingPaneLayout
    {
        public bool IsSlideEnable { private get; set; } = true;

        public Ys_SlidingPaneLayout(Context context) : base(context)
        {
        }

        public Ys_SlidingPaneLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public Ys_SlidingPaneLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }

        protected Ys_SlidingPaneLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (MotionEventCompat.GetActionMasked(ev) == (int)MotionEventActions.Move)
            {
                if (!IsSlideEnable)
                    return false;
            }
            return base.OnInterceptTouchEvent(ev);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (MotionEventCompat.GetActionMasked(e) == (int)MotionEventActions.Move)
            {
                if (!IsSlideEnable)
                    return false;
            }
            return base.OnTouchEvent(e);
        }

    }
}
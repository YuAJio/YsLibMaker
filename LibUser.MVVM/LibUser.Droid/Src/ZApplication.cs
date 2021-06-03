using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LibUser.MVVM.Core;

using MvvmCross.Platforms.Android.Core;
using MvvmCross.Platforms.Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUser.Droid.Src
{
    [Application]
    public class ZApplication : MvxAndroidApplication<MvxAndroidSetup<CommandCenter>, CommandCenter>
    {
        public ZApplication() { }

        public ZApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
    }
}
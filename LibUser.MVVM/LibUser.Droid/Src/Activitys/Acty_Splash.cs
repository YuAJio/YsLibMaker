using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using MvvmCross.Platforms.Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUser.Droid.Src.Activitys
{
    [Activity(
        MainLauncher = true,
        Icon = "@mipmap/ic_launcher",
        Label = "Acty_Splash",
        Theme = "@style/Theme.Splash",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Acty_Splash : MvxSplashScreenActivity
    {
    }
}
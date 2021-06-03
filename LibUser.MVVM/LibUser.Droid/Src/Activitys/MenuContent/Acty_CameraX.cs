using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LibUser.MVVM.Core.ViewModels.MenuContent;

using MvvmCross.Platforms.Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUser.Droid.Src.Activitys.MenuContent
{
    [Activity(Label = "Acty_CameraX",
        Theme = "@style/Theme.Standard",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Acty_CameraX : MvxActivity<ViewM_CameraX>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ViewModel.ViewAct_PageClose += delegate
            {
                this.Finish();
            };
        }



    }
}
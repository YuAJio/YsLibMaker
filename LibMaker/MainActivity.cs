using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Ys.BeLazy;
using Android.Views;
using System;
using System.Threading.Tasks;
using System.Threading;
using Android.Support.V7.Widget;
using Ys.BeLazy.AdvanceWithTheTimes;
using Android.Graphics;

namespace LibMaker
{
    [Activity(Label = "@string/app_name", Theme = "@style/JK.SwipeBack.Transparent.Theme", MainLauncher = true)]
    public class MainActivity : BaseSwipeBackActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.activity_main);

            var bt = FindViewById<Button>(Resource.Id.bt_next);

            bt.Click += delegate
            {
                bt.Text = GetSystemVersion();
            };
        }

        private string GetSystemVersion()
        {
            return Android.OS.Build.VERSION.BaseOs;
        }

    }
}


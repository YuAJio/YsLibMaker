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
            this.IsAllowSlidClose = true;
            base.OnCreate(savedInstanceState);
        }

        public override int A_GetContentViewId()
        {
            return Resource.Layout.activity_main;
        }

        public override void B_BeforeInitView()
        {

        }

        public override void C_InitView()
        {
            var bt = FindViewById<Button>(Resource.Id.bt_next);
            bt.Click += delegate
            {
                bt.Text = GetSystemVersion();
            };
        }

        public override void D_BindEvent()
        {

        }

        public override void E_InitData()
        {

        }

        public override void F_OnClickListener(View v, EventArgs e)
        {

        }

        private string GetSystemVersion()
        {
            return Android.OS.Build.VERSION.BaseOs;
        }

    }
}


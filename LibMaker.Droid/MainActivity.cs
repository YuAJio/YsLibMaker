﻿using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Net;
using Android.Views;
using System;
using System.Threading.Tasks;
using AndroidX.Camera.View;
using Ys.Camera.Droid.Views;

namespace LibMaker.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Ys.BeLazy.YsBaseActivity
    {
        private YsCameraX _CameraX;

        public override int A_GetContentViewId()
        {
            return Resource.Layout.Activity_Main;
        }

        public override void B_BeforeInitView()
        {

        }

        public override void C_InitView()
        {
            _CameraX = FindViewById<YsCameraX>(Resource.Id.cxCameraX);

            _CameraX.InitAndStartCamera(this);

            FindViewById<Button>(Resource.Id.bt_event).Click += delegate (object sender, EventArgs e)
            {
                var jk = Android.OS.Environment.GetExternalStoragePublicDirectory(System.IO.Path.Combine(Android.OS.Environment.DirectoryPictures, Resources.GetString(Resource.String.app_name)));

                var path = System.IO.Path.Combine(jk.AbsolutePath, "PIC", $"{Guid.NewGuid()}.jpg");
                _CameraX.TakePicture(path,
                    (onError) =>
                    {

                    },
                    (onSaved) =>
                    {

                    }
                    );
                //ShowWaitDialog_Normal("新的哦~~  大的哦~~");
                //Task.Run(async () =>
                //{
                //    await Task.Delay(2 * 1000);
                //}).ContinueWith(x =>
                //{
                //    HideWaitDiaLog();
                //    if (x.Exception != null)
                //        return;
                //}, TaskScheduler.FromCurrentSynchronizationContext());
            };

            FindViewById<Button>(Resource.Id.bt_event).LongClick += delegate (object sender, View.LongClickEventArgs ex)
            {
                _CameraX.SwitchFacing(this);
                //ShowWaitDialog_Samll("新的哦~~ 小的哦~~");
                //Task.Run(async () =>
                //{
                //    await Task.Delay(2 * 1000);
                //}).ContinueWith(x =>
                //{
                //    HideWaitDiaLog();
                //    if (x.Exception != null)
                //        return;
                //}, TaskScheduler.FromCurrentSynchronizationContext());
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


    }
}
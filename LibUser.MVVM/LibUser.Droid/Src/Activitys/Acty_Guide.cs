using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using LibUser.Droid.Src.Activitys.MenuContent;
using LibUser.MVVM.Core.Models;
using LibUser.MVVM.Core.ViewModels;

using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUser.Droid.Src.Activitys
{
    [Activity(
        Label = "Activity_Guide",
        Theme = "@style/Theme.Standard",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Acty_Guide : MvxActivity<ViewM_Guide>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Activity_Guide);
            FindViewById<MvxRecyclerView>(Resource.Id.rvMenu).SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false)); ;
            ViewModel.InitAndCreateMenu(CreateMenu());
            ViewModel.ViewAct_GotoNewPage += Go2Activity;
        }


        private List<Mod_GuildMenu> CreateMenu()
        {
            return
                new List<Mod_GuildMenu>
                {
                   new Mod_GuildMenu { MenuName = "CameraX测试(图片分类)", ViewType = typeof(Acty_CameraX) }
                };
        }

        private void Go2Activity(Type type)
        {
            StartActivity(new Intent(this, type));
        }

    }
}
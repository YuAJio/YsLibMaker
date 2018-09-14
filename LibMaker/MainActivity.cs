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

            var rl_father = FindViewById<RelativeLayout>(Resource.Id.rl_father);

            var random = new Java.Util.Random();
            var red = random.NextInt(255);
            var green = random.NextInt(255);
            var blue = random.NextInt(255);

            rl_father.SetBackgroundColor(Color.Argb(255, red, green, blue));

            var bt_next = FindViewById<Button>(Resource.Id.bt_next);

            bt_next.Click -= Bt_next_Click;
            bt_next.Click += Bt_next_Click;

        }

        private void Bt_next_Click(object sender, EventArgs e)
        {
            this.StartActivity(new Android.Content.Intent(this, typeof(MainActivity)));
        }
    }
}


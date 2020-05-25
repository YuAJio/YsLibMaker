using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LibMaker
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { TAG_BROADCAST_IF })]
    public class UpdateReciver : BroadcastReceiver
    {
        public const string TAG_BROADCAST_IF = "com.yurishi.belazy.AUAVBC";
        public override void OnReceive(Context context, Intent intent)
        {
            var state = intent.GetStringExtra("state");
            var result = intent.GetStringExtra("result");
        }
    }
}
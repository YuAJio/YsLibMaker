using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ys.Bluetooth.Droid;

namespace LibUser.BluetoothBle
{
    public class BleManager
    {
        #region 单例
        private static readonly Lazy<BleManager> instance = new Lazy<BleManager>(() => new BleManager());
        private BleManager() { }
        public static BleManager Instance
        {
            get
            {
                return instance.Value;
            }
        }
        #endregion


    }
}
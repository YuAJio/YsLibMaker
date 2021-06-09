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

namespace Ys.Bluetooth.Droid
{
    public class YsBluetoothManager
    {
        #region 单例
        private static readonly Lazy<YsBluetoothManager> instance = new Lazy<YsBluetoothManager>(() => new YsBluetoothManager());
        private YsBluetoothManager() { }
        public static YsBluetoothManager Instance
        {
            get
            {
                return instance.Value;
            }
        }
        #endregion



    }
}
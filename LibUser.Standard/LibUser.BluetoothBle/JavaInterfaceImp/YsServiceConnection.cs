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

namespace LibUser.BluetoothBle.JavaInterfaceImp
{
    public class YsServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public event Action<ComponentName, IBinder> Act_OnServiceConnected;
        public event Action<ComponentName> Act_OnServiceDisconnected;

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Act_OnServiceConnected?.Invoke(name, service);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Act_OnServiceDisconnected?.Invoke(name);
        }
    }
}
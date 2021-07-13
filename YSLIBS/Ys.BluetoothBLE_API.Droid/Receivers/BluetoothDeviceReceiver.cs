using Android.Bluetooth;
using Android.Content;

using System;

namespace Ys.BluetoothBLE_API.Droid.Receivers
{
    [BroadcastReceiver]
    public class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public event EventHandler<BleEventArg> BleReceiveEvent;
        public BluetoothAdapter BleAdapter => BluetoothAdapter.DefaultAdapter;

        public override void OnReceive(Context context, Intent intent)
        {
            if (BleAdapter == null)
            {
                BleReceiveEvent?.Invoke(this, new BleEventArg { EventCode = BleEventCode.BluetoothNotSupport });
                return;
            }

            var action = intent.Action;

            switch (action)
            {
                case BluetoothDevice.ActionFound:
                    {
                        var rssi = intent.Extras.GetShort(BluetoothDevice.ExtraRssi);
                        var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        BleReceiveEvent?.Invoke(this, new BleEventArg { EventCode = BleEventCode.FoundNew, FounedDevice = device, Rssi = rssi });
                    }
                    break;
                case BluetoothAdapter.ActionDiscoveryStarted:
                    {
                        BleReceiveEvent?.Invoke(this, new BleEventArg { EventCode = BleEventCode.DiscoveryStart });
                    }
                    break;
                case BluetoothAdapter.ActionDiscoveryFinished:
                    {
                        BleReceiveEvent?.Invoke(this, new BleEventArg { EventCode = BleEventCode.DiscoveryFinished });
                    }
                    break;
            }
        }

        public class BleEventArg
        {
            public BleEventCode EventCode { get; set; }
            public BluetoothDevice FounedDevice { get; set; }
            public int Rssi { get; set; }
        }

        public enum BleEventCode
        {
            FoundNew,
            DiscoveryStart,
            DiscoveryFinished,
            NotOpenBluetooth,
            BluetoothNotSupport,
        }
    }
}
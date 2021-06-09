using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Com.Ble.Ble;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibUser.BluetoothBle.JavaInterfaceImp
{
    public class YsBleCallBack : BleCallBack
    {
        #region EventArgs
        public event EventHandler<string> OnConnectedEvent;
        public event EventHandler<string> OnConnectTimeoutEvent;
        public event EventHandler<EventModel_SII> OnConnectionErrorEvent;
        public event EventHandler<string> OnDisconnectedEvent;
        public event EventHandler<EventModel_SII> OnServicesUndiscoveredEvent;
        public event EventHandler<string> OnServicesDiscoveredEvent;
        public event EventHandler<EventModel_SII> OnReadRemoteRssiEvent;
        public event EventHandler<EventModel_SBI> OnCharacteristicWriteEvent;
        public event EventHandler<EventModel_SBI> OnCharacteristicChangedEvent;
        #endregion

        public class EventModel_SII
        {
            public string Mac { get; set; }
            public int Status { get; set; }
            public int NewStatus { get; set; }
        }

        public class EventModel_SBI
        {
            public string Mac { get; set; }
            public BluetoothGattCharacteristic bluetoothGattCharacteristic { get; set; }
            public int Status { get; set; }
        }

        public override void OnConnected(string p0)
        {
            OnConnectedEvent?.Invoke(this, p0);
        }

        public override void OnConnectTimeout(string p0)
        {
            OnConnectTimeoutEvent?.Invoke(this, p0);
        }

        public override void OnConnectionError(string p0, int p1, int p2)
        {
            OnConnectionErrorEvent?.Invoke(this, new EventModel_SII { Mac = p0, Status = p1, NewStatus = p2 });
        }

        public override void OnDisconnected(string p0)
        {
            OnDisconnectedEvent?.Invoke(this, p0);
        }


        public override void OnServicesUndiscovered(string p0, int p1)
        {
            OnServicesUndiscoveredEvent?.Invoke(this, new EventModel_SII { Mac = p0, Status = p1 });
        }

        public override void OnServicesDiscovered(string p0)
        {
            OnServicesDiscoveredEvent?.Invoke(this, p0);
        }

        public override void OnReadRemoteRssi(string p0, int p1, int p2)
        {
            OnReadRemoteRssiEvent?.Invoke(this, new EventModel_SII { Mac = p0, Status = p1, NewStatus = p2 });
        }

        public override void OnCharacteristicWrite(string p0, BluetoothGattCharacteristic p1, int p2)
        {
            OnCharacteristicWriteEvent?.Invoke(this, new EventModel_SBI { Mac = p0, bluetoothGattCharacteristic = p1, Status = p2 });
        }

        public override void OnCharacteristicChanged(string p0, BluetoothGattCharacteristic p1)
        {
            OnCharacteristicChangedEvent?.Invoke(this, new EventModel_SBI { Mac = p0, bluetoothGattCharacteristic = p1 });
        }


    }
}
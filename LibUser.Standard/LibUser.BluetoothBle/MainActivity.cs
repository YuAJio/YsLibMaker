using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;

using Com.Ble.Ble;

using LibUser.BluetoothBle.JavaInterfaceImp;

using System;
using System.Collections.Generic;
using System.Linq;

using Ys.Bluetooth.Droid;

namespace LibUser.BluetoothBle
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private YsBluetoothListRecycerAd ysRecyclerViewAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            InitView();

            InitBluetoothEngie();
        }

        private void InitView()
        {
            var rv = FindViewById<RecyclerView>(Resource.Id.rvBlueTooth);
            rv.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
            ysRecyclerViewAdapter = new YsBluetoothListRecycerAd(this);
            ysRecyclerViewAdapter.ItemClickEventArg += YsAdapter_ItemClickEventArg;
            rv.SetAdapter(ysRecyclerViewAdapter);

            FindViewById<TextView>(Resource.Id.tvEpigraph).Click += delegate
           {
               if (!_Receiver.BleAdapter.IsDiscovering)
                   _Receiver.BleAdapter.StartDiscovery();
               new System.Threading.Thread(new System.Threading.ThreadStart(() =>
               {
                   System.Threading.Thread.Sleep(8 * 1000);
                   if (_Receiver.BleAdapter.IsDiscovering)
                       _Receiver.BleAdapter.CancelDiscovery();
               })).Start();
           };
        }

        private void YsAdapter_ItemClickEventArg(object sender, int e)
        {
            var device = List_BDevices.Where(x => x.Address == ysRecyclerViewAdapter.DataList[e].BtAdress).FirstOrDefault();
            ConnectToBluetooth(device);
        }

        #region 蓝牙相关

        private List<BluetoothDevice> List_BDevices = new List<BluetoothDevice>();
        private bool isConnecting = false;
        private BleService mLeService;
        private string ConnectedBluetoothDeviceMAC;

        private void InitBluetoothEngie()
        {
            _Receiver = new BluetoothDeviceReceiver();
            _Receiver.BleReceiveEvent += Receiver_BleReceiveEvent;
            var servceConncection = new YsServiceConnection();
            var bleCallBack = new YsBleCallBack();
            servceConncection.Act_OnServiceDisconnected += delegate
            {
                mLeService = null;
            };
            servceConncection.Act_OnServiceConnected += delegate (ComponentName j, IBinder k)
            {
                mLeService = ((BleService.LocalBinder)k).GetService(bleCallBack);
                mLeService.SetDecode(false);
                mLeService.Initialize();
            };
            BindService(new Intent(this, typeof(BleService)), servceConncection, Bind.AutoCreate);

            bleCallBack.OnConnectedEvent += BleCallBack_OnConnectedEvent;
            bleCallBack.OnConnectTimeoutEvent += BleCallBack_OnConnectTimeoutEvent;
            bleCallBack.OnConnectionErrorEvent += BleCallBack_OnConnectionErrorEvent;
            bleCallBack.OnDisconnectedEvent += BleCallBack_OnDisconnectedEvent;
            bleCallBack.OnServicesUndiscoveredEvent += BleCallBack_OnServicesUndiscoveredEvent;
            bleCallBack.OnServicesDiscoveredEvent += BleCallBack_OnServicesDiscoveredEvent;
            bleCallBack.OnCharacteristicChangedEvent += BleCallBack_OnCharacteristicChangedEvent;
        }

        private void ConnectToBluetooth(BluetoothDevice bluetooth)
        {
            try
            {
                if (!isConnecting)
                {
                    isConnecting = true;
                    if (bluetooth != null && mLeService != null)
                    {
                        if (!string.IsNullOrEmpty(ConnectedBluetoothDeviceMAC))
                            mLeService.SetAutoConnect(ConnectedBluetoothDeviceMAC, false);

                        mLeService.Connect(bluetooth.Address, true);
                        ConnectedBluetoothDeviceMAC = bluetooth.Address;
                        ConnectedBluetoothDeviceMAC = string.IsNullOrEmpty(ConnectedBluetoothDeviceMAC) ? "" : ConnectedBluetoothDeviceMAC;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region BleCallBackEventHandler

        private void BleCallBack_OnConnectedEvent(object sender, string e)
        {
            isConnecting = false;
        }

        private void BleCallBack_OnConnectTimeoutEvent(object sender, string e)
        {
            isConnecting = false;
        }

        private void BleCallBack_OnConnectionErrorEvent(object sender, YsBleCallBack.EventModel_SII e)
        {
            isConnecting = false;
        }

        private void BleCallBack_OnDisconnectedEvent(object sender, string e)
        {
        }

        private void BleCallBack_OnServicesUndiscoveredEvent(object sender, YsBleCallBack.EventModel_SII e)
        {
        }

        private void BleCallBack_OnServicesDiscoveredEvent(object sender, string e)
        {
        }

        private void BleCallBack_OnCharacteristicChangedEvent(object sender, YsBleCallBack.EventModel_SBI e)
        {
        }

        #endregion BleCallBackEventHandler

        #region 蓝牙广播接收器相关

        private BluetoothDeviceReceiver _Receiver;

        private void RegisterBluetoothReceiver()
        {
            RegisterReceiver(_Receiver, new IntentFilter(BluetoothDevice.ActionFound));
            RegisterReceiver(_Receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
            RegisterReceiver(_Receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
        }

        private void UnRegisterBluetoothReceiver()
        {
            UnregisterReceiver(_Receiver);
        }

        private void Receiver_BleReceiveEvent(object sender, BluetoothDeviceReceiver.BleEventArg e)
        {
            switch (e.EventCode)
            {
                case BluetoothDeviceReceiver.BleEventCode.FoundNew:
                    {
                        if (List_BDevices.Any(x => x.Address == e.FounedDevice.Address))
                            return;
                        List_BDevices.Add(e.FounedDevice);

                        ysRecyclerViewAdapter.DataList.Add(new Mod_BlueTooth
                        {
                            BtAdress = e.FounedDevice.Address,
                            BtName = e.FounedDevice.Name,
                            BtConnected = e.FounedDevice.BondState != Android.Bluetooth.Bond.None,
                            Rssi = e.Rssi,
                        });
                        ysRecyclerViewAdapter.NotifyDataSetChanged();
                    }
                    break;
            }
        }

        #endregion 蓝牙广播接收器相关

        #endregion 蓝牙相关

        protected override void OnResume()
        {
            base.OnResume();
            RegisterBluetoothReceiver();
        }

        protected override void OnStop()
        {
            base.OnStop();
            UnRegisterBluetoothReceiver();
        }

        #region 蓝牙列表适配器

        private class Mod_BlueTooth
        {
            public string BtName { get; set; }
            public string BtAdress { get; set; }
            public bool BtConnected { get; set; }
            public int Rssi { get; set; }
        }

        private class YsBluetoothListRecycerAd : RecyclerView.Adapter
        {
            private Context context;

            private List<Mod_BlueTooth> _dataList;

            public event EventHandler<int> ItemClickEventArg;

            public YsBluetoothListRecycerAd(Context context)
            {
                this.context = context;
                _dataList = new List<Mod_BlueTooth>();
            }

            public List<Mod_BlueTooth> DataList
            {
                get { return _dataList; }
                set
                {
                    _dataList = value;
                    NotifyDataSetChanged();
                }
            }

            public override int ItemCount { get { return DataList == null || !DataList.Any() ? 0 : DataList.Count; } }

            public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
            {
                var holder = vh as ViewHolder;
                var data = DataList[position];
                if (data == null)
                    return;
                holder.tvName.Text = string.IsNullOrEmpty(data.BtName) ? "Nameless" : data.BtName;
                holder.tvAdress.Text = data.BtAdress;
                holder.ivState.Selected = data.BtConnected;

                holder.ItemView.Tag = position;
                holder.ItemView.Click -= ItemView_Click;
                holder.ItemView.Click += ItemView_Click;
            }

            private void ItemView_Click(object sender, EventArgs e)
            {
                var positioni = (int)(sender as View).Tag;
                ItemClickEventArg?.Invoke(this, positioni);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return new ViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.listitem_bluetooth, parent, false));
            }

            private class ViewHolder : RecyclerView.ViewHolder
            {
                public TextView tvName;
                public TextView tvAdress;
                public ImageView ivState;

                public ViewHolder(View itemView) : base(itemView)
                {
                    tvName = ItemView.FindViewById<TextView>(Resource.Id.tvName);
                    tvAdress = ItemView.FindViewById<TextView>(Resource.Id.tvCode);
                    ivState = ItemView.FindViewById<ImageView>(Resource.Id.ivState);
                }
            }
        }

        #endregion 蓝牙列表适配器
    }
}
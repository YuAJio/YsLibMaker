﻿using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;


using System;
using System.Collections.Generic;
using System.Linq;

using Ys.Bluetooth.Droid;
using Ys.BluetoothBLE_API.Droid;
using Ys.BluetoothBLE_API.Droid.Manager;

using static Ys.BluetoothBLE_API.Droid.Enum_Republic;

namespace LibUser.BluetoothBle
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private YsBluetoothListRecycerAd ysRecyclerViewAdapter;

        private TextView tvOutput;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            InitView();

            InitBluetoothReciver();
            InitBLEAPI();
        }

        private void InitView()
        {
            tvOutput = FindViewById<TextView>(Resource.Id.tvOutPut);
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
            FindViewById<Button>(Resource.Id.btSendCmd).Click += delegate
            {//发送指令
                YsBleManager.Instance.SendDebugTestDataCmd(2);
            };
        }

        private void YsAdapter_ItemClickEventArg(object sender, int e)
        {
            var device = List_BDevices.Where(x => x.Address == ysRecyclerViewAdapter.DataList[e].BtAdress).FirstOrDefault();
            ConnectToBluetooth(device);
        }

        #region 蓝牙相关
        private List<BluetoothDevice> List_BDevices = new List<BluetoothDevice>();
        private void InitBluetoothReciver()
        {
            _Receiver = new BluetoothDeviceReceiver();
            _Receiver.BleReceiveEvent += Receiver_BleReceiveEvent;
        }

        private void ConnectToBluetooth(BluetoothDevice bluetooth)
        {
            YsBleManager.Instance.ConnectToBleDevice(bluetooth?.Address);
        }



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

        #region 框架蓝牙的调用
        private void InitBLEAPI()
        {
            YsBleManager.Instance.Act_OnLeDeviceConnectChange -= OnLeDeviceConnectChangeInvoke;
            YsBleManager.Instance.Act_OnLeDeviceConnectChange += OnLeDeviceConnectChangeInvoke;

            YsBleManager.Instance.OnResponseEvent -= Instance_OnResponseEvent;
            YsBleManager.Instance.OnResponseEvent += Instance_OnResponseEvent;

            YsBleManager.Instance.InitBleService(this);
        }

        private void Instance_OnResponseEvent(object sender, Ys.BluetoothBLE_API.Droid.Models.ResponseArg e)
        {
            RunOnUiThread(() =>
            {
                tvOutput.Text = $"Cmd:{e.Cmd}\nMessage:{ Newtonsoft.Json.JsonConvert.SerializeObject(e.HexDatas)}";
                if (e.Cmd == Constants_Republic.TEST_CMD)
                {
                    var success = Constants_Republic.RESULT_OK == e.HexDatas[0];
                    if (!success)
                        Toast.MakeText(this, "检测结果回调失败,调试一下吧 ", ToastLength.Long).Show();

                    var result = YsBleManager.Instance.ProcessDetectResult(e.HexDatas, 1);
                }
            });
        }

        private void OnLeDeviceConnectChangeInvoke(BleConnectState state)
        {
            RunOnUiThread(() =>
            {
                switch (state)
                {
                    case BleConnectState.WaitForConnect:
                        Toast.MakeText(this, "等待蓝牙设备的连接", ToastLength.Short).Show();
                        break;
                    case BleConnectState.Connecting:
                        Toast.MakeText(this, "蓝牙设备连接中", ToastLength.Short).Show();
                        break;
                    case BleConnectState.ConnectionError:
                        Toast.MakeText(this, "蓝牙设备连接失败", ToastLength.Short).Show();
                        break;
                    case BleConnectState.ConnectionTimeOut:
                        Toast.MakeText(this, "蓝牙连接超时,请重试", ToastLength.Short).Show();
                        break;
                    case BleConnectState.Connected:
                        Toast.MakeText(this, "蓝牙设备连接成功!!!!!我们胜利了!", ToastLength.Short).Show();
                        break;
                    case BleConnectState.DisConnect:
                        Toast.MakeText(this, "蓝牙设备已经断开连接,大清亡了", ToastLength.Short).Show();
                        break;
                }
            });
        }

        #endregion

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
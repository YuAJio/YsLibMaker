using Android.App;
using Android.App.Assist;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using Java.Util;

using LibUser.MVVM.Core.ViewModels.MenuContent;

using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Views;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Ys.Bluetooth.Droid;

using static Android.Bluetooth.BluetoothClass;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace LibUser.Droid.Src.Activitys.MenuContent
{
    [Activity(Label = "Acty_CameraX",
        Theme = "@style/Theme.Standard",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Acty_BlueTooth : MvxActivity<ViewM_BlueTooth>
    {
        private YsBluetoothAdapter ysAdapter;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Activity_BlueTooth);

            var rv = FindViewById<RecyclerView>(Resource.Id.rvBlueTooth);
            rv.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
            ysAdapter = new YsBluetoothAdapter(this);
            ysAdapter.ItemClickEventArg += YsAdapter_ItemClickEventArg;
            rv.SetAdapter(ysAdapter);
            InvokeViewModel();

            _Receiver = new BluetoothDeviceReceiver();
            _Receiver.BleReceiveEvent -= Receiver_BleReceiveEvent;
            _Receiver.BleReceiveEvent += Receiver_BleReceiveEvent;
        }

        private void YsAdapter_ItemClickEventArg(object sender, int e)
        {
            var thatDevice = List_MemoryBle.Where(x => x.Address == ysAdapter.DataList[e].BtCode).FirstOrDefault();
            Ble_ConnectToBle(thatDevice);
        }

        private void InvokeViewModel()
        {
            this.ViewModel.ActionEvent_ScanBlueTooth += delegate
            {
                if (!_Receiver.BleAdapter.IsDiscovering)
                    _Receiver.BleAdapter.StartDiscovery();
                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    System.Threading.Thread.Sleep(8 * 1000);
                    if (_Receiver.BleAdapter.IsDiscovering)
                        _Receiver.BleAdapter.CancelDiscovery();
                })).Start();
                //Managers.YsBluetoothManager.Instance.StartScan(this, (j, k, m) =>
                //{
                //    if (adapter.DataList.Any(x => x.BtCode == j.Address))
                //        return;
                //    adapter.DataList.Add(new MVVM.Core.Models.Mod_BlueTooth
                //    {
                //        BtCode = j.Address,
                //        BtName = j.Name,
                //        BtConnected = j.BondState != Android.Bluetooth.Bond.None
                //    });

                //    var jk = adapter.DataList.Where(x => x.BtCode == OneAndOnlyBlue).FirstOrDefault();
                //    if (jk != null)
                //    {

                //    }
                //    adapter.NotifyDataSetChanged();
                //});
            };
        }


        #region 蓝牙广播相关
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
                        if (ysAdapter.DataList.Any(x => x.BtCode == e.FounedDevice.Address))
                            return;
                        ysAdapter.DataList.Add(new MVVM.Core.Models.Mod_BlueTooth
                        {
                            BtCode = e.FounedDevice.Address,
                            BtName = e.FounedDevice.Name,
                            BtConnected = e.FounedDevice.BondState != Android.Bluetooth.Bond.None,
                            Rssi = e.Rssi,
                        });
                        ysAdapter.DataList = ysAdapter.DataList.OrderBy(x => x.Rssi).ToList();
                        List_MemoryBle.Add(e.FounedDevice);
                        ysAdapter.NotifyDataSetChanged();
                    }
                    break;
                case BluetoothDeviceReceiver.BleEventCode.DiscoveryStart:
                    break;
                case BluetoothDeviceReceiver.BleEventCode.DiscoveryFinished:
                    break;
            }
        }

        #endregion

        #region 蓝牙相关
        private List<BluetoothDevice> List_MemoryBle = new List<BluetoothDevice>();
        private BluetoothSocket BleSocket = null;
        private System.IO.Stream bleStream;
        public bool SocketConnected { get; set; }

        private void Ble_ConnectToBle(BluetoothDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("Connect Device Is Null");

            new System.Threading.Thread(new System.Threading.ThreadStart(async () =>
            {
                try
                {
                    //var jk = List_MemoryBle.Select(x => new { Name = x.Name, Mac = x.Address, Type = x.Type });
                    //var js = Newtonsoft.Json.JsonConvert.SerializeObject(jk);
                    //var thatdevice = _Receiver.BleAdapter.GetRemoteDevice(device.Address);

                    //var serialUUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");

                    //BluetoothSocket socket = thatdevice.CreateRfcommSocketToServiceRecord(serialUUID);
                    //socket.Connect();

                    var jk = ConnectToDevcie(device);
                    //BleSocket = (BluetoothSocket)Java.Lang.Class.FromType(typeof(BluetoothDevice))
                    //.GetDeclaredMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Class.FromType(typeof(Java.Lang.Integer)) })
                    //.Invoke(device, new Java.Lang.Object[] { 1 });
                    //BleSocket = jk.CreateInsecureRfcommSocketToServiceRecord(Java.Util.UUID.RandomUUID()/*("00001101-0000-1000-8000-00805F9B34FB")*/);
                    //new System.Threading.Thread(new System.Threading.ThreadStart(async () =>
                    //{
                    //    _Receiver.BleAdapter.CancelDiscovery();
                    //    await BleSocket.ConnectAsync();
                    //    bleStream = BleSocket.InputStream;
                    //    BleSocket.Close();
                    //})).Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            })).Start();
        }

        private async Task<bool> ConnectToDevcie(BluetoothDevice device)
        {
            var _ct = new CancellationTokenSource();
            while (!_ct.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(200);
                    var jk = _Receiver.BleAdapter.BondedDevices.ToList();

                    if (device == null)
                        return false;

                    var uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    BleSocket = device.CreateRfcommSocketToServiceRecord(uuid);

                    if (BleSocket == null)
                        return false;

                    new Thread(new ThreadStart(async () =>
                    {
                        await BleSocket.ConnectAsync();
                    })).Start();

                    SocketConnected = BleSocket.IsConnected;
                    if (SocketConnected)
                        Console.WriteLine("YESYES");
                    return SocketConnected;
                }
                catch (Exception ex)
                {
                    BleSocket.Close();
                    return false;
                }
            }
            return false;
        }
        #endregion


        protected override void OnResume()
        {
            base.OnResume();
            RegisterBluetoothReceiver();
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnRegisterBluetoothReceiver();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if ((BleSocket?.IsConnected).GetValueOrDefault()) BleSocket?.Close();
        }

        #region 适配器
        private class YsBluetoothAdapter : RecyclerView.Adapter
        {
            private Context context;

            private List<MVVM.Core.Models.Mod_BlueTooth> _dataList;

            public event EventHandler<int> ItemClickEventArg;
            public YsBluetoothAdapter(Context context)
            {
                this.context = context;
                _dataList = new List<MVVM.Core.Models.Mod_BlueTooth>();
            }

            public List<MVVM.Core.Models.Mod_BlueTooth> DataList
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
                holder.tvName.Text = string.IsNullOrEmpty(data.BtName) ? "暂无名称" : data.BtName;
                holder.tvAdress.Text = data.BtCode;
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

        #endregion


    }
}
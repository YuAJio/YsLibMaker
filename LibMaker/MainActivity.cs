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
//using Net.Posprinter.Service;
using Android.Content;
//using Net.Posprinter.Posprinterface;
using Android.Bluetooth;
using System.Linq;
using System.Collections.Generic;
//using Net.Posprinter.Utils;
using Android.Runtime;
using Prism;
using Prism.Ioc;
using YS_BTPrint;

namespace LibMaker
{
    [Activity(Label = "@string/app_name", Theme = "@style/JK.SwipeBack.Transparent.Theme", MainLauncher = true)]
    public class MainActivity : BaseSwipeBackActivity
    {
        private PrintUtils Printer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.IsAllowSlidClose = true;
            base.OnCreate(savedInstanceState);

            var intent = new Intent(this, typeof(Ys.BeLazy.Services.Ser_AutoUpdateApplicationVersion));
            intent.PutExtra(Ys.BeLazy.Services.Ser_AutoUpdateApplicationVersion.TAG_BROADCASTACTION,UpdateReciver.TAG_BROADCAST_IF);
            StartService(intent);
        }

        public override int A_GetContentViewId()
        {
            return Resource.Layout.activity_main;
        }

        public override void B_BeforeInitView()
        {

        }

        public override void C_InitView()
        {
            var bt = FindViewById<Button>(Resource.Id.bt_next);
            bt.Click += delegate
            {
                //if (IsConnect)
                //{
                //    //测试打印
                //    PrintText();
                //}
                //else
                //{
                //    if (!btAdapter.IsEnabled)
                //        Console.WriteLine("此处提示未打开蓝牙功能,提示去打开");
                //    else
                //        ConnectBuleTooth();
                //}
            };
        }

        public override void D_BindEvent()
        {
        }

        public override void E_InitData()
        {
            //InitPrinter();
        }

        public override void F_OnClickListener(View v, EventArgs e)
        {

        }

        private string GetSystemVersion()
        {
            return Android.OS.Build.VERSION.BaseOs;
        }



        #region 打印控件相关
        //private IMyBinder myBinder;

        //private bool IsConnect = false;

        ///// <summary>
        ///// 初始化蓝牙打印模块
        ///// </summary>
        //private void InitPrinter()
        //{
        //    this.BindService(
        //        new Intent(
        //            this,
        //            typeof(PosprinterService)),
        //        new ServiceConnectionInvoke((j) =>
        //        {
        //            myBinder = j as IMyBinder;
        //        }),
        //        Bind.AutoCreate);
        //    btAdapter = BluetoothAdapter.DefaultAdapter;
        //}


        //private BluetoothAdapter btAdapter;

        ///// <summary>
        ///// 连接到蓝牙打印机
        ///// </summary>
        //private void ConnectBuleTooth()
        //{
        //    if (!btAdapter.IsDiscovering)
        //        btAdapter.StartDiscovery();

        //    var btReceiver = new BlueToothDeviceReceiver();
        //    btReceiver.OnReceiveAct -= OnBTReceiverReActInvoke;
        //    btReceiver.OnReceiveAct += OnBTReceiverReActInvoke;
        //    RegisterReceiver(btReceiver, new IntentFilter(BluetoothDevice.ActionFound));
        //    RegisterReceiver(btReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));

        //    FintAvailableDevice();
        //}

        ///// <summary>
        ///// 寻找可用的蓝牙设备
        ///// </summary>
        //private void FintAvailableDevice()
        //{
        //    if (btAdapter == null)
        //        return;

        //    var devices = btAdapter.BondedDevices;
        //    if (devices != null)
        //    {
        //        if (devices.Any())
        //        {
        //            var thatOne = devices.Where(x => x.Name.Contains("Printer")).FirstOrDefault();
        //            if (thatOne != null)
        //                ConnectOrDisConnectBlueTooth(thatOne.Address, true);
        //        }

        //    }


        //}

        ///// <summary>
        ///// 连接或是解除连接蓝牙设备
        ///// </summary>
        ///// <param name="mac"></param>
        ///// <param name="isConnect"></param>
        ///// <param name="isRe"></param>
        //private void ConnectOrDisConnectBlueTooth(string mac, bool isConnect, bool isRe = false)
        //{
        //    if ((btAdapter?.IsDiscovering).GetValueOrDefault())
        //        btAdapter?.CancelDiscovery();
        //    if (string.IsNullOrEmpty(mac))
        //        return;

        //    try
        //    {
        //        if (isConnect)
        //        {//连接蓝牙设备
        //            if (!IsConnect)
        //                myBinder?.ConnectBtPort(mac, new TaskCallBackInvoke((isOk) =>
        //                {
        //                    if (isOk)
        //                    {
        //                        IsConnect = true;
        //                        Console.WriteLine("连接成功");
        //                    }
        //                    else
        //                        Console.WriteLine("连接失败");
        //                }));
        //            else
        //                ConnectOrDisConnectBlueTooth(mac, false, true);
        //        }
        //        else
        //        {//断开蓝牙设备
        //            if (IsConnect)
        //                myBinder?.DisconnectCurrentPort(new TaskCallBackInvoke((isOk) =>
        //                {
        //                    if (isOk)
        //                    {
        //                        IsConnect = false;
        //                        Console.WriteLine("断开成功");
        //                        if (isRe)
        //                            ConnectOrDisConnectBlueTooth(mac, true);
        //                    }
        //                    else
        //                        Console.WriteLine("断开失败");
        //                }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //}

        ///// <summary>
        ///// 蓝牙接收者实现者
        ///// </summary>
        ///// <param name="intent"></param>
        //private void OnBTReceiverReActInvoke(Intent intent)
        //{
        //    switch (intent.Action)
        //    {
        //        case BluetoothDevice.ActionFound:
        //            {//找到了设备 
        //                var btd = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;
        //                if (btd.BondState != Bond.Bonded)
        //                {//如果这个设备没被配对
        //                    if (btd.Name.Contains("Printer"))
        //                    {//如果找到了
        //                        ConnectOrDisConnectBlueTooth(btd.Address, true);
        //                    }
        //                }

        //            }
        //            break;
        //        case BluetoothAdapter.ActionDiscoveryFinished:
        //            {//搜索结束 

        //            }
        //            break;
        //    }

        //}



        //private void PrintText()
        //{
        //    try
        //    {

        //        myBinder.WriteSendData(new TaskCallBackInvoke((isOk) =>
        //        {

        //        }), new ProcessDataInvoke(() =>
        //        {
        //            var list = new List<byte[]>
        //        {
        //                 DataForSendToPrinterPos58.InitializePrinter(),
        //            //设置初始位置
        //            DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 20 ),
        //            //字体放大一倍
        //            DataForSendToPrinterPos58.SelectCharacterSize( 17 ),
        //            StringUtils.StrTobytes("JASMINERAINING"),
        //            DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 20 ),
        //            StringUtils.StrTobytes("sasda"),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            DataForSendToPrinterPos58.InitializePrinter(),
        //            DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            StringUtils.StrTobytes("ffff"),
        //            DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            StringUtils.StrTobytes("dddd"),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            ////设置初始位置
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 20 ),
        //            ////字体放大一倍
        //            //DataForSendToPrinterPos58.SelectCharacterSize( 17 ),
        //            //StringUtils.StrTobytes("キャラ"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 20 ),
        //            //StringUtils.StrTobytes("キャスト"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("巽幸太郎"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("宮野真守"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("源さくら"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("本渡楓"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("二階堂サキ"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("田野アサミ"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("水野愛"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("種田梨沙"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("紺野純子"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("河瀬茉希"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("ゆうぎり"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("衣川里佳"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("星川リリィ"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("田中美海"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            //DataForSendToPrinterPos58.InitializePrinter(),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 50 , 0 ),
        //            //StringUtils.StrTobytes("山田たえ"),
        //            //DataForSendToPrinterPos58.SetAbsolutePrintPosition( 200 , 0 ),
        //            //StringUtils.StrTobytes("三石琴乃"),
        //            //DataForSendToPrinterPos58.PrintAndFeedLine(),

        //            DataForSendToPrinterPos58.PrintAndFeedLine(),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),
        //            DataForSendToPrinterPos58.PrintAndFeedLine(),
        //    };
        //            return list;
        //        }));
        //    }
        //    catch (Java.Lang.RuntimeException ex)
        //    {

        //    }



        //}



        //private class BlueToothDeviceReceiver : BroadcastReceiver
        //{
        //    public Action<Intent> OnReceiveAct { get; set; }

        //    public override void OnReceive(Context context, Intent intent)
        //    {
        //        OnReceiveAct?.Invoke(intent);
        //    }
        //}

        //private class ServiceConnectionInvoke : Java.Lang.Object, IServiceConnection
        //{
        //    public Action<IBinder> onServiceConnectedAct;

        //    public ServiceConnectionInvoke(Action<IBinder> onServiceConnectedAct)
        //    {
        //        this.onServiceConnectedAct = onServiceConnectedAct;
        //    }

        //    public void OnServiceConnected(ComponentName name, IBinder service)
        //    {
        //        onServiceConnectedAct?.Invoke(service);
        //    }

        //    public void OnServiceDisconnected(ComponentName name)
        //    {
        //    }
        //}


        //private class TaskCallBackInvoke : Java.Lang.Object, ITaskCallback
        //{
        //    private readonly Action<bool> CallBackAct;

        //    public TaskCallBackInvoke(Action<bool> callBackAct)
        //    {
        //        CallBackAct = callBackAct;
        //    }

        //    public void OnFailed()
        //    {
        //        CallBackAct?.Invoke(false);
        //    }
        //    public void OnSucceed()
        //    {
        //        CallBackAct?.Invoke(true);
        //    }
        //}

        //private class ProcessDataInvoke : Java.Lang.Object, IProcessData
        //{
        //    private readonly Func<IList<byte[]>> ProcessDataFunc;

        //    public ProcessDataInvoke(Func<IList<byte[]>> processDataFunc)
        //    {
        //        ProcessDataFunc = processDataFunc;
        //    }

        //    public IList<byte[]> ProcessDataBeforeSend()
        //    {
        //        return ProcessDataFunc?.Invoke();
        //    }
        //}
        #endregion



    }
}


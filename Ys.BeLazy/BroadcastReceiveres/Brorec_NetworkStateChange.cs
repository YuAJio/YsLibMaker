using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using System;

namespace Ys.BeLazy.BroadcastReceiveres
{

    /// <summary>
    /// 网络变化监听广播
    /// </summary>
    [BroadcastReceiver]
    public class Brorec_NetworkStateChange : BroadcastReceiver
    {
        public Brorec_NetworkStateChange()
        {
        }

        public Brorec_NetworkStateChange(Context context) : base()
        {
            CheckNetworkState(context);
        }

        public Brorec_NetworkStateChange(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            CheckNetworkState(context);
        }

        private void CheckNetworkState(Context context)
        {
            var cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            if (context != null)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // 类型或成员已过时
                    var mWiFiNetworkInfo = cm.ActiveNetworkInfo;
                    if (mWiFiNetworkInfo != null)
                        Invoke(mWiFiNetworkInfo.Type, mWiFiNetworkInfo.IsConnected);
                    else
                        Invoke(ConnectivityType.Wifi | ConnectivityType.Mobile | ConnectivityType.Ethernet, false);
#pragma warning restore CS0618 // 类型或成员已过时
                }
                else
                {
                    var network = cm.ActiveNetwork;
                    if (network != null)
                    {
                        var nc = cm.GetNetworkCapabilities(network);
                        if (nc != null)
                        {
#pragma warning disable CS0618 // 类型或成员已过时
                            if (nc.HasTransport(TransportType.Wifi)) Invoke(ConnectivityType.Wifi, (cm.ActiveNetworkInfo?.IsConnected).GetValueOrDefault());
                            else if (nc.HasTransport(TransportType.Ethernet)) Invoke(ConnectivityType.Ethernet, (cm.ActiveNetworkInfo?.IsConnected).GetValueOrDefault());
                            else if (nc.HasTransport(TransportType.Cellular)) Invoke(ConnectivityType.Mobile, (cm.ActiveNetworkInfo?.IsConnected).GetValueOrDefault());
                            else Invoke(ConnectivityType.Wifi | ConnectivityType.Mobile | ConnectivityType.Ethernet, (cm.ActiveNetworkInfo?.IsConnected).GetValueOrDefault());
#pragma warning restore CS0618 // 类型或成员已过时
                        }
                        else
                            Invoke(ConnectivityType.Wifi | ConnectivityType.Mobile | ConnectivityType.Ethernet, false);
                    }
                }
            }

        }

        public static Action<ConnectivityType, bool> Act_NetworkChange { get; set; }
        private void Invoke(ConnectivityType type, bool isConnect)
        {
            Act_NetworkChange?.Invoke(type, isConnect);
        }

        public IntentFilter NormalIntentFilter
        {
            get
            {
                var @if = new IntentFilter();
                @if.AddAction("android.net.ethernet.ETHERNET_STATE_CHANGED");
                @if.AddAction("android.net.ethernet.STATE_CHANGE");
                @if.AddAction("android.net.conn.CONNECTIVITY_CHANGE");//检测接入进来的网络
                @if.AddAction(Android.Net.Wifi.WifiManager.NetworkStateChangedAction);//检测连接的WIFI改变
                return @if;
            }
        }

    }

}
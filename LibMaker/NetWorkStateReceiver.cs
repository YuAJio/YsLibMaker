using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LibMaker
{
    //[BroadcastReceiver]
    public class NetWorkStateReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                //获得ConnectivityManager对象
                ConnectivityManager connMgr = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);

                //获取ConnectivityManager对象对应的NetworkInfo对象
                //获取WIFI连接的信息
                NetworkInfo wifiNetworkInfo = connMgr.GetNetworkInfo(ConnectivityType.Wifi);
                //获取移动数据连接的信息
                NetworkInfo dataNetworkInfo = connMgr.GetNetworkInfo(ConnectivityType.Mobile);
                if (wifiNetworkInfo.IsConnected && dataNetworkInfo.IsConnected)
                    Toast.MakeText(context, "WIFI已连接,移动数据已连接", ToastLength.Long).Show();
                else if (wifiNetworkInfo.IsConnected && !dataNetworkInfo.IsConnected)
                    Toast.MakeText(context, "WIFI已连接,移动数据已断开", ToastLength.Long).Show();
                else if (!wifiNetworkInfo.IsConnected && dataNetworkInfo.IsConnected)
                    Toast.MakeText(context, "WIFI已断开,移动数据已连接", ToastLength.Long).Show();
                else
                    Toast.MakeText(context, "WIFI已断开,移动数据已断开", ToastLength.Long).Show();
                //API大于23时使用下面的方式进行网络监听
            }
            else
            {
                //获得ConnectivityManager对象
                ConnectivityManager connMgr = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);

                //获取所有网络连接的信息
                Network[] networks = connMgr.GetAllNetworks();
                //用于存放网络连接信息
                StringBuilder sb = new StringBuilder();
                //通过循环将网络信息逐个取出来
                for (int i = 0; i < networks.Length; i++)
                {
                    //获取ConnectivityManager对象对应的NetworkInfo对象
                    NetworkInfo networkInfo = connMgr.GetNetworkInfo(networks[i]);
                    sb.Append(networkInfo.TypeName + " connect is " + networkInfo.IsConnected);
                }
                Toast.MakeText(context, sb.ToString(), ToastLength.Long).Show();
            }
        }
    }
}
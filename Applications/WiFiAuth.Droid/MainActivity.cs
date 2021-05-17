using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Net;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace WiFiAuth.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WiFiAuthMain);

            InitView();
            BindViewEvent();
            Act_GetNetState += delegate (string j)
            {
                RunOnUiThread(() =>
                {
                    tvInfo.Text = j;
                });
            };
        }

        #region Views
        private Button btEvent;
        private TextView tvInfo;

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitView()
        {
            btEvent = FindViewById<Button>(Resource.Id.btEvent);
            tvInfo = FindViewById<TextView>(Resource.Id.tvInfo);
        }
        /// <summary>
        /// 绑定控件事件
        /// </summary>
        private void BindViewEvent()
        {
            tvInfo.LongClick += delegate
            {
                tvInfo.Text = "";
            };
            btEvent.Click += BtEvent_Click;
        }


        /// <summary>
        /// 控件点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtEvent_Click(object sender, System.EventArgs e)
        {
            new Thread(async () =>
            {
                await IsWIFISetPortal();
            }).Start();
        }
        #endregion

        #region Action 
        private Action<string> Act_GetNetState;
        #endregion


        private async Task<bool> IsWIFISetPortal()
        {
            var mWalledGardenUrl = "http://g.cn/generate_204";
            var WALLED_GARDEN_SOCKET_TIMEOUT_MS = 10 * 1000;
            IPStatus iPStatus = IPStatus.Unknown;
            HttpURLConnection httpURLConnection = null;
            try
            {
                var url = new URL(mWalledGardenUrl);
                httpURLConnection = (HttpURLConnection)url?.OpenConnection();
                httpURLConnection.InstanceFollowRedirects = false;
                httpURLConnection.ConnectTimeout = WALLED_GARDEN_SOCKET_TIMEOUT_MS;
                httpURLConnection.ReadTimeout = WALLED_GARDEN_SOCKET_TIMEOUT_MS;
                httpURLConnection.UseCaches = false;
                var ping = new Ping();
                var result_Ping = await ping.SendPingAsync("https://www.baidu.com", 3000);
                iPStatus = result_Ping.Status;
                //if (httpURLConnection.ResponseCode == HttpStatus.NoContent)
                //{
                Act_GetNetState?.Invoke($"PingResut : {iPStatus}\nResCode : {httpURLConnection.ResponseCode}\nResMessage : {httpURLConnection.ResponseMessage}");
                //}
                //else
                //    Act_GetNetState?.Invoke($"ResCode : {httpURLConnection.ResponseCode}\nResMessage : {httpURLConnection.ResponseMessage}");
                return httpURLConnection.ResponseCode != HttpStatus.NoContent;
            }
            catch (Exception ex)
            {
                Act_GetNetState?.Invoke($"PingResut : {iPStatus}\nException : {ex.Message}");
                return false;
            }
            finally
            {
                if (httpURLConnection != null)
                    httpURLConnection.Disconnect();
            }

        }

    }
}
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Amap.Api.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ys.GaoDeMap_Droid
{
    [Service]
    public class Service_GaoDeMapLocation : Service
    {
        public Action<AMapLocation> Act_OnLocationChanged { get; set; }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            InitLocationClient();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            DestoryLocationClient();
        }

        #region 定位系统相关
        /// <summary>
        /// 定位对象
        /// </summary>
        public AMapLocationClient _LocaltionClient = null;


        private void InitLocationClient()
        {
            _LocaltionClient = new AMapLocationClient(ApplicationContext);
            _LocaltionClient.SetLocationListener(new MapLocaltionListenerImp(OnLocationChanged));

            //创建定位设置
            var option = new AMapLocationClientOption();
            //设置定位间隔,单位毫秒,默认为2000ms，最低1000ms。
            option.SetInterval(15 * 1000);
            //单位是毫秒，默认30000毫秒，建议超时时间不要低于8000毫秒。
            option.SetHttpTimeOut(10 * 1000);
            //关闭缓存机制
            option.SetLocationCacheEnable(false);
            if (null != _LocaltionClient)
            {
                _LocaltionClient.SetLocationOption(option);
                _LocaltionClient.StopLocation();
                _LocaltionClient.StartLocation();
            }



        }
        private void DestoryLocationClient() => _LocaltionClient?.OnDestroy();

        private void StopLocation() => _LocaltionClient?.StopLocation();

        private void ReStartLocation()
        {
            if (!(_LocaltionClient?.IsStarted).GetValueOrDefault())
                _LocaltionClient?.StartLocation();
        }

        private void OnLocationChanged(AMapLocation aMapLocation)
        {
#if DEBUG
            Console.WriteLine($"=============高德地图定位结果:{aMapLocation.ToStr()}=============");
#endif
            Act_OnLocationChanged?.Invoke(aMapLocation);
        }



        private class MapLocaltionListenerImp : Java.Lang.Object, IAMapLocationListener
        {
            public Action<AMapLocation> OnLocationChangeAct;

            public MapLocaltionListenerImp(Action<AMapLocation> onLocationChangeAct)
            {
                OnLocationChangeAct = onLocationChangeAct;
            }

            public void OnLocationChanged(AMapLocation mapLocation) => OnLocationChangeAct?.Invoke(mapLocation);
        }
        #endregion



    }
}
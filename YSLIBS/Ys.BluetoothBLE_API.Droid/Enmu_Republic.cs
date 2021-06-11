using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid
{
    public class Enum_Republic
    {
        public enum BleConnectState
        {
            WaitForConnect,
            Connecting,
            ConnectionError,
            ConnectionTimeOut,
            Connected,
            DisConnect,
        }

        public enum DetectResult
        {
            /// <summary>
            /// 阴性
            /// </summary>
            Negative = 0,
            /// <summary>
            /// 弱阳性
            /// </summary>
            Weakly_reactive =1,
            /// <summary>
            /// 阳性
            /// </summary>
            Positive = 2,
        }
    }
}

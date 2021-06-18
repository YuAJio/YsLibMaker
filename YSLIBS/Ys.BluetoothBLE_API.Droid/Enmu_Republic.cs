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
            DEFAULT = 0,
            /// <summary>
            ///  阴性
            /// </summary>
            Negative = 1,
            /// <summary>
            /// 阳性
            /// </summary>
            Positive = 2,
        }

        public enum JudgeType
        {
            Area = 1,
            Height = 2,
        }

        public enum CTDriection
        {
            Positive = 1,
            Ngative = 2,
        }
    }
}

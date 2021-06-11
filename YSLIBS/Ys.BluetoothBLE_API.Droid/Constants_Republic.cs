using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid
{
    public class Constants_Republic
    {
        public const string RESULT_OK = "01";
        public const string RESULT_NG = "00";
        public const string NOT_CONNECTED = "-99";
        public const byte CONNECTED_STATUS = 11;
        public const byte FIRMWARE_UPLOAD = 88;

        /// <summary>
        /// LED亮度
        /// </summary>
        public const byte LED_CMD = 0x21;
        /// <summary>
        /// 更新固件指令
        /// </summary>
        public const byte FIRMWARE_CMD = 0x22;
        /// <summary>
        /// 自动断电时间设置
        /// </summary>
        public const byte POWER_CMD = 0x23;
        /// <summary>
        /// 读取检测结果
        /// </summary>
        public const byte TEST_CMD = 0x01;
        /// <summary>
        /// 读取检测结果＋所有曲线数据
        /// </summary>
        public const byte TEST_WITH_ALL_DATA = 0x02;
        /// <summary>
        /// 
        /// </summary>
        public const byte POWER_STATUS_CMD = 0x26;
    }
}

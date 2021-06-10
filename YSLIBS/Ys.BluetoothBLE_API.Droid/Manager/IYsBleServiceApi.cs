using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid.Manager
{
    public interface IYsBleServiceApi
    {
        protected const string RESULT_OK = "01";
        protected const string RESULT_NG = "00";
        protected const string NOT_CONNECTED = "-99";
        protected const byte CONNECTED_STATUS = 11;
        protected const byte FIRMWARE_UPLOAD = 88;

        /// <summary>
        /// LED亮度
        /// </summary>
        protected const byte LED_CMD = 0x21;
        /// <summary>
        /// 更新固件指令
        /// </summary>
        protected const byte FIRMWARE_CMD = 0x22;
        /// <summary>
        /// 自动断电时间设置
        /// </summary>
        protected const byte POWER_CMD = 0x23;
        /// <summary>
        /// 读取检测结果
        /// </summary>
        protected const byte TEST_CMD = 0x01;
        /// <summary>
        /// 读取检测结果＋所有曲线数据
        /// </summary>
        protected const byte TEST_WITH_ALL_DATA = 0x02;
        /// <summary>
        /// 
        /// </summary>
        protected const byte POWER_STATUS_CMD = 0x26;


        void UpdateFirmwareCmd(int fileLength);
        void UploadFirmwateData(byte[] datas);
        void SendTestCmd(Byte itemCount);
        void SendDebugTestDataCmd(Byte itemCount);
        void SendPowerTimeCmd(byte value);
        void SendLEDBrightnessCmd(byte value);
        void SendPowerStatusCmd();

    }
}

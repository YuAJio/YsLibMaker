using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid.Manager
{
    public interface IYsBleServiceApi
    {
        void UpdateFirmwareCmd(int fileLength);
        void UploadFirmwateData(byte[] datas);
        void SendTestCmd(Byte itemCount);
        void SendDebugTestDataCmd(Byte itemCount);
        void SendPowerTimeCmd(byte value);
        void SendLEDBrightnessCmd(byte value);
        void SendPowerStatusCmd();

    }
}

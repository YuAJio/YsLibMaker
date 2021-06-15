using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;

using Com.Ble.Api;
using Com.Ble.Ble;
using Com.Ble.Ble.Constants;
using Com.Ble.Ble.Util;

using System;
using System.Linq;
using System.Threading;

using Ys.BluetoothBLE_API.Droid.JavaInterfaceImp;
using Ys.BluetoothBLE_API.Droid.Models;
using Ys.BluetoothBLE_API.Droid.Tools;

using static Ys.BluetoothBLE_API.Droid.Enum_Republic;

namespace Ys.BluetoothBLE_API.Droid.Manager
{
    [Service]
    public class YsBleManager : IYsBleServiceApi
    {

        #region 回调
        /// <summary>
        /// LE蓝牙设备连接状态改变通知
        /// </summary>
        public event Action<BleConnectState> Act_OnLeDeviceConnectChange;
        public event EventHandler<ResponseArg> OnResponseEvent;
        #endregion

        private BleService mLeService;
        private YsServiceConnection servceConncection;

        private string ConnectedBluetoothDeviceMAC;
        /// <summary>
        /// 是否是连接中
        /// </summary>
        private bool IsConnecting = false;
        /// <summary>
        /// 是否是连接状态
        /// </summary>
        public bool IsConnected { get; set; }
        public void InitBleService(Context context)
        {
            IsConnecting = false;
            servceConncection = new YsServiceConnection();
            var bleCallBack = new YsBleCallBack();
            servceConncection.Act_OnServiceDisconnected += delegate
            {
                mLeService = null;
            };
            servceConncection.Act_OnServiceConnected += delegate (ComponentName j, IBinder k)
            {
                mLeService = ((BleService.LocalBinder)k).GetService(bleCallBack);
                mLeService.SetDecode(false);
                mLeService.ConnectTimeout = 5 * 1000;
                mLeService.Initialize();
            };
            context.BindService(new Intent(context, typeof(BleService)), servceConncection, Bind.AutoCreate);

            bleCallBack.OnConnectedEvent += BleCallBack_OnConnectedEvent;
            bleCallBack.OnConnectTimeoutEvent += BleCallBack_OnConnectTimeoutEvent;
            bleCallBack.OnConnectionErrorEvent += BleCallBack_OnConnectionErrorEvent;
            bleCallBack.OnDisconnectedEvent += BleCallBack_OnDisconnectedEvent;
            bleCallBack.OnServicesUndiscoveredEvent += BleCallBack_OnServicesUndiscoveredEvent;
            bleCallBack.OnServicesDiscoveredEvent += BleCallBack_OnServicesDiscoveredEvent;
            bleCallBack.OnCharacteristicChangedEvent += BleCallBack_OnCharacteristicChangedEvent;
        }

        public void RelaseBleService(Context context)
        {
            context.UnbindService(servceConncection);
        }

        public void ConnectToBleDevice(string leMacAdress)
        {
            try
            {
                if (!IsConnecting)
                {
                    IsConnecting = true;
                    if (!string.IsNullOrEmpty(leMacAdress) && mLeService != null)
                    {
                        if (!string.IsNullOrEmpty(ConnectedBluetoothDeviceMAC))
                            mLeService.SetAutoConnect(ConnectedBluetoothDeviceMAC, false);

                        mLeService.Connect(leMacAdress, true);
                        Act_OnLeDeviceConnectChange.Invoke(BleConnectState.Connecting);
                        ConnectedBluetoothDeviceMAC = leMacAdress;
                        ConnectedBluetoothDeviceMAC = string.IsNullOrEmpty(ConnectedBluetoothDeviceMAC) ? "" : ConnectedBluetoothDeviceMAC;
                    }
                }
            }
            catch (Exception ex)
            {
                IsConnecting = false;
                Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.ConnectionError);
                throw ex;
            }
        }


        #region BleCallBackEventHandler

        private void BleCallBack_OnConnectedEvent(object sender, string e)
        {
            IsConnecting = false;
            IsConnected = true;
        }

        private void BleCallBack_OnConnectTimeoutEvent(object sender, string e)
        {
            IsConnecting = false;
            IsConnected = false;
            Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.ConnectionTimeOut);
        }

        private void BleCallBack_OnConnectionErrorEvent(object sender, YsBleCallBack.EventModel_SII e)
        {
            IsConnecting = false;
            IsConnected = false;
            Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.ConnectionError);

        }

        private void BleCallBack_OnDisconnectedEvent(object sender, string e)
        {
            IsConnected = false;
            Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.DisConnect);

        }


        private void BleCallBack_OnServicesDiscoveredEvent(object sender, string e)
        {
            IsConnected = true;
            Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.Connected);
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(300);
                EnableNotification(e, BleUUIDS.PrimaryService, BleUUIDS.Characters[1]);
            })).Start();
        }

        private void BleCallBack_OnServicesUndiscoveredEvent(object sender, YsBleCallBack.EventModel_SII e)
        {
            IsConnected = false;
            Act_OnLeDeviceConnectChange?.Invoke(BleConnectState.DisConnect);

        }


        private void BleCallBack_OnCharacteristicChangedEvent(object sender, YsBleCallBack.EventModel_SBI e)
        {
            ProcessInputData(e.bluetoothGattCharacteristic.GetValue());
        }

        #endregion BleCallBackEventHandler


        private bool EnableNotification(string address, Java.Util.UUID serUuid, Java.Util.UUID charUuit)
        {
            if (mLeService == null)
                return false;
            var gatt = mLeService.GetBluetoothGatt(address);
            var c = GattUtil.GetGattCharacteristic(gatt, serUuid, charUuit);
            return SetCharacteristieNotification(gatt, c, true);
        }

        private bool SetCharacteristieNotification(BluetoothGatt gatt, BluetoothGattCharacteristic c, bool enable)
        {
            if (mLeService != null)
            {
                return mLeService.SetCharacteristicNotification(gatt, c, enable);
            }
            return false;
        }


        private int ReceiveDataCount = 0;
        private string ReciveDataTotal = "";
        private string BufferPackage = "";
        private void ProcessInputData(byte[] reciveBytes)
        {
            try
            {
                ReceiveDataCount += reciveBytes.Length;
                var receiveData = DataUtil.ByteArrayToHex(reciveBytes);
                BufferPackage += receiveData + " ";
                var split = "5C 72 5C 6E";
                if (BufferPackage.IndexOf(split) > -1)
                {
                    var currentPackage = BufferPackage.Substring(0, BufferPackage.IndexOf(split)).Trim();
                    BufferPackage = BufferPackage[(BufferPackage.IndexOf(split) + split.Length)..].Trim();
                    var sendPackage = currentPackage;
                    new Thread(new ThreadStart(() =>
                    {
                        SendPorcessResponse(sendPackage);
                    })).Start();
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region 实现接口命令
        public void UpdateFirmwareCmd(int fileLength)
        {
            SendRequestToBLE(Constants_Republic.FIRMWARE_CMD, fileLength);
        }

        public void UploadFirmwateData(byte[] datas)
        {
            SendRequestToBLE(Constants_Republic.FIRMWARE_UPLOAD, datas);
        }

        public void SendTestCmd(byte itemCount)
        {
            SendRequestToBLE(Constants_Republic.TEST_CMD, itemCount);
        }

        public void SendDebugTestDataCmd(byte itemCount)
        {
            SendRequestToBLE(Constants_Republic.TEST_WITH_ALL_DATA, itemCount);
        }

        public void SendPowerTimeCmd(byte value)
        {
            SendRequestToBLE(Constants_Republic.POWER_CMD, value);
        }

        public void SendLEDBrightnessCmd(byte value)
        {
            SendRequestToBLE(Constants_Republic.LED_CMD, value);
        }

        public void SendPowerStatusCmd()
        {
            SendRequestToBLE(Constants_Republic.POWER_STATUS_CMD, null);
        }
        #endregion

        #region 向蓝牙机器发送消息
        public void SendRequestToBLE(byte cmd, int value)
        {
            if (IsConnected)
            {
                var sendBytes = LeSocketPackageWriter.Write(cmd, value);
                mLeService.Send(ConnectedBluetoothDeviceMAC, sendBytes, false);
            }
            else
                SendPorcessResponse(cmd, Constants_Republic.NOT_CONNECTED);
        }

        public void SendRequestToBLE(byte cmd, byte value)
        {
            if (IsConnected)
            {
                var sendBytes = LeSocketPackageWriter.Write(cmd, value);
                mLeService.Send(ConnectedBluetoothDeviceMAC, sendBytes, false);
            }
            else
                SendPorcessResponse(cmd, Constants_Republic.NOT_CONNECTED);
        }

        public void SendRequestToBLE(byte cmd, byte[] value)
        {
            if (IsConnected)
            {
                var sendBytes = LeSocketPackageWriter.Write(cmd, value);
                mLeService.Send(ConnectedBluetoothDeviceMAC, sendBytes, false);
            }
            else
                SendPorcessResponse(cmd, Constants_Republic.NOT_CONNECTED);
        }
        public void SendRequestToBLE(byte cmd)
        {
            if (IsConnected)
            {
                var sendBytes = LeSocketPackageWriter.Write(cmd, (string)null);
                mLeService.Send(ConnectedBluetoothDeviceMAC, sendBytes, false);
            }
            else
                SendPorcessResponse(cmd, Constants_Republic.NOT_CONNECTED);
        }
        #endregion

        #region 处理接收到的蓝牙设备消息
        private void SendPorcessResponse(string receiveData)
        {
            var allHexString = receiveData.Split(" ");
            if (allHexString == null && allHexString.Length <= 0)
                return;
            var cmd = (byte)Java.Lang.Integer.ParseInt(allHexString[0], 16);
            var listHexString = allHexString.ToList();
            listHexString.RemoveAt(0);
            var dataString = listHexString.ToArray();
            SendPorcessResponse(cmd, dataString);
        }

        private void SendPorcessResponse(byte cmd, string data)
        {
            OnResponseEvent?.Invoke(this, new ResponseArg
            {
                Cmd = cmd,
                HexDatas = new string[] { data }
            });
        }
        private void SendPorcessResponse(byte cmd, string[] data)
        {
            OnResponseEvent?.Invoke(this, new ResponseArg
            {
                Cmd = cmd,
                HexDatas = data
            });
        }

        #endregion

    }
}

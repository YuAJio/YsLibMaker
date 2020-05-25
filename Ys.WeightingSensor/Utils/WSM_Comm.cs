using Modbus.Device;
using System;
using System.IO.Ports;
using System.Threading;


namespace Ys.WeightingSensor_Modbus.Utils
{
    public class Comm
    {
        private static bool linkMode = true;
        private static bool grossMode = true;
        private static SerialPort serialPort;
        private static ModbusSerialMaster master;

        public static SerialPort GetSerialPort()
        {
            if (serialPort == null)
            {
                serialPort = new SerialPort();
            }
            return serialPort;
        }

        public static string[] GetPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            return ports;
        }

        public static ModbusSerialMaster GetModbus()
        {
            if (master == null)
            {
                master = ModbusSerialMaster.CreateRtu(GetSerialPort());
                master.Transport.ReadTimeout = 15;
                master.Transport.WriteTimeout = GetReadTimeOut();
                master.Transport.Retries = 0;
                master.Transport.WaitToRetryMilliseconds = 100;
            }
            return master;
        }

        public static bool GetStatus()
        {
            return GetSerialPort().IsOpen;
        }

        public static bool IsLinkMode()
        {
            return linkMode;
        }

        public static bool IsGrossMode()
        {
            return grossMode;
        }

        public static void SetLinkMode(bool linkMode)
        {
            Comm.linkMode = linkMode;
            int timeout = linkMode ? 20 : GetWeightTimeOut();
            if (timeout != GetModbus().Transport.ReadTimeout)
            {
                GetModbus().Transport.ReadTimeout = timeout;
            }
        }

        public static void SetGrossMode(bool grossMode)
        {
            Comm.grossMode = grossMode;
            int timeout = grossMode ? GetWeightTimeOut() : GetReadTimeOut();
            if (timeout != GetModbus().Transport.ReadTimeout)
            {
                GetModbus().Transport.ReadTimeout = timeout;
            }
        }

        public static bool OpenPort()
        {
            try
            {
                GetSerialPort().PortName = Param.Port;
                GetSerialPort().BaudRate = Param.Baud;
                GetSerialPort().Parity = GetParity(Param.Parity);
                GetSerialPort().DataBits = 8;
                GetSerialPort().StopBits = StopBits.Two;
                GetSerialPort().Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Parity GetParity(int parity)
        {
            Parity p;
            switch (parity)
            {
                case 0:
                default:
                    p = Parity.None;
                    break;
                case 1:
                    p = Parity.Odd;
                    break;
                case 2:
                    p = Parity.Even;
                    break;
                case 3:
                    p = Parity.Space;
                    break;
            }
            return p;
        }

        public static void ClosePort()
        {
            GetSerialPort().Close();
        }

        public static bool StartLink()
        {
            try
            {
                ushort[] data = new ushort[] { 0x4743, 0xA9A9, 0x5656, 0x0001, 0x9207 };
                GetModbus().WriteMultipleRegisters(Param.GetAddr(), 0x3000, data);
                return true;
            }
            catch
            {

            }
            return false;
        }

        public static bool StopLink()
        {
            try
            {
                ushort[] data = new ushort[] { 0x4743, 0xA9A9, 0x5656, 0x0001, 0x0000 };
                GetModbus().WriteMultipleRegisters(Param.GetAddr(), 0x3000, data);
                return true;
            }
            catch
            {

            }
            return false;
        }

        public static bool SaveParam()
        {
            try
            {
                if (Param.Tool)
                {
                    ushort[] data = new ushort[] { 0x4743, 0x8181, 0x7E7E, 0x0000 };
                    GetModbus().WriteMultipleRegisters(1, 0x3000, data);
                    return true;
                }
                else
                {
                    ushort[] data = new ushort[] { 0x4743, 0x3434, 0xCBCB, 0x0000 };
                    GetModbus().WriteMultipleRegisters(Param.GetAddr(), 0x1000, data);
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }

        public static int WriteRegisters(ushort address, ushort[] data)
        {
            try
            {
                GetModbus().WriteMultipleRegisters(Param.GetAddr(), address, data);
                return 0;
            }
            catch (Modbus.SlaveException e)
            {
                return e.SlaveExceptionCode;
            }
            catch (TimeoutException)
            {
                return -2;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static ushort[] ReadRegisters(ushort address, ushort length)
        {
            try
            {
                return GetModbus().ReadHoldingRegisters(Param.GetAddr(), address, length);
            }
            catch
            {

            }
            return null;
        }

        public static ushort[] ReadWeight(ushort address, ushort length)
        {
            try
            {
                return GetModbus().ReadHoldingRegisters(Param.GetAddr(), address, length);
            }
            catch
            {

            }
            return null;
        }

        public static int WriteCoil(ushort address, bool coil)
        {
            try
            {
                GetModbus().WriteSingleCoil(Param.GetAddr(), address, coil);
                return 0;
            }
            catch (Modbus.SlaveException e)
            {
                return e.SlaveExceptionCode;
            }
            catch (TimeoutException)
            {
                return -2;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static ushort[] ReadInputRegisters(ushort address, ushort length)
        {
            try
            {
                return GetModbus().ReadInputRegisters(Param.GetAddr(), address, length);
            }
            catch
            {

            }
            return null;
        }

        public static bool CheckTool()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    ushort[] result = GetModbus().ReadInputRegisters(248, 0x0000, 1);
                    if (result != null && result.Length == 1 && result[0] == 0x1234)
                    {
                        return true;
                    }
                    Thread.Sleep(10);
                }
                catch
                {

                }
            }
            return false;
        }

        public static int GetWeightSpan()
        {
            int index = Array.IndexOf(Param.PORT_BAUD_ITEM, Param.Baud.ToString());
            index = index < 0 ? 0 : index;
            return (index == 0 && !Param.Adapt) ? 200 : 100;
        }

        public static int GetWeightTimeOut()
        {
            int index = Array.IndexOf(Param.PORT_BAUD_ITEM, Param.Baud.ToString());
            index = index < 0 ? 0 : index;
            return (index == 0 && !Param.Adapt) ? 160 : 80;
        }

        public static int GetReadTimeOut()
        {
            int index = Array.IndexOf(Param.PORT_BAUD_ITEM, Param.Baud.ToString());
            index = index < 0 ? 0 : index;
            return (index == 0 && !Param.Adapt) ? 1200 : 800;
        }
    }
}

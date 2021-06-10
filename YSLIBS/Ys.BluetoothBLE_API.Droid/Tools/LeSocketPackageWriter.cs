using Com.Ble.Api;

using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid.Tools
{
    public class LeSocketPackageWriter
    {
        public static byte[] Write(byte cmd, string hexStr)
        {
            try
            {
                var contents = string.IsNullOrEmpty(hexStr) ? null : DataUtil.HexToByteArray(hexStr);
                return Write(cmd, contents);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static byte[] Write(byte cmd, byte[] contents)
        {
            try
            {
                byte[] cmdBy = null;
                if (cmd! > 0)
                    cmdBy = new byte[] { cmd };
                if (contents == null || contents.Length <= 0)
                    return cmdBy;
                return Combile(cmdBy, contents);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static byte[] Write(byte cmd, int content)
        {
            try
            {
                var contents = content <= 0 ? null : IntToHexByteArray(content);
                return Write(cmd, contents);
            }
            catch (Exception)
            {

            }
            return null;
        }


        public static byte[] Write(byte cmd, byte content)
        {
            try
            {
                if (content > 0)
                {
                    var contetns = DataUtil.HexToByteArray(Java.Lang.Integer.ToHexString(content));
                    return Write(cmd, contetns);
                }
                return Write(cmd, (byte[])null);

            }
            catch (Exception e)
            {

            }
            return null;
        }


        public static byte[] Combile(byte[] params1, byte[] params2)
        {
            var listBytes = new List<byte>();
            listBytes.AddRange(params1);
            listBytes.AddRange(params2);
            return listBytes.ToArray();
        }

        public static byte[] IntToHexByteArray(int @params)
        {
            var hexStr = Java.Lang.Integer.ToHexString(@params);
            var bytes = DataUtil.HexToByteArray(hexStr);
            return bytes;
        }

        public static int HexByteArrayToInt(byte[] bytes)
        {
            var text = DataUtil.ByteArrayToHex(bytes);
            text = text.Replace(" ", "");
            return Java.Lang.Integer.ParseInt(text, 16);
        }

    }
}

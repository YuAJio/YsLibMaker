using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid.Models
{
    public partial class ResponseArg
    {
        public string[] HexDatas { get; set; }
        public byte Cmd { get; set; }
    }
}

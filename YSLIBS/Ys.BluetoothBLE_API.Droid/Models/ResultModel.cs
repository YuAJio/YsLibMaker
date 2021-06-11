using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Ys.BluetoothBLE_API.Droid.Enum_Republic;

namespace Ys.BluetoothBLE_API.Droid.Models
{
    public class ResultModel
    {
        public int Position { get; set; }
        public int Height { get; set; }
        public int Area { get; set; }

        public string Value { get; set; }
        public string AreaValue { get; set; }

        public DetectResult Result { get; set; }
    }
}
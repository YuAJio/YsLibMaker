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

namespace Ys.BluetoothBLE_API.Droid.Models
{
    public class TestResultUI
    {
        public bool IsTestSucces { get; set; }
        public string TestFailedReson { get; set; }
        public bool IsNegative { get; set; }
        public string Name { get; set; }
        public List<TestResultUIItem> TestItems { get; set; }
    }
    public class TestResultUIItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsNegative { get; set; }
    }
}
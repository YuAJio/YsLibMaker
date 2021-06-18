using System;
using System.Collections.Generic;
using System.Text;

using static Ys.BluetoothBLE_API.Droid.Enum_Republic;

namespace Ys.BluetoothBLE_API.Droid.Models
{
    public class TestStrip
    {
        public string Name { get; set; }
        public int TestCount { get; set; }

        public JudgeType JudgeType { get; set; }
        public CTDriection CTDriection { get; set; }

        public float PositiveValue { get; set; }
        public float NegativeValue { get; set; }
        public List<StripItemList> StripItemList { get; set; }
    }

    public class StripItemList
    {
        public string Name { get; set; }
    }

}

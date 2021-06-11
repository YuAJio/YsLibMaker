using System;
using System.Collections.Generic;
using System.Text;

namespace Ys.BluetoothBLE_API.Droid.Models
{
    public class TestStrip
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int TestCount { get; set; }

        public int JudgeType { get; set; }
        public int CtDirection { get; set; }
        public int CategoryId { get; set; }
        public int Idx { get; set; }

        public  decimal PositiveValue { get; set; }
        public decimal NegativeValue { get; set; }

        public List<int> ItemIdList { get; set; }
    }

}

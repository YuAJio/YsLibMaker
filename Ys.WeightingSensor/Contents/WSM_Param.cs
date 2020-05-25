using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ys.WeightingSensor.Contents
{
   public class WSM_Param
    {
        public static readonly bool IS_DEBUG = false;

        public static readonly string[] PORT_BAUD_ITEM = { "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200" };
        public static readonly string[] DIV_ITEM = { "1", "2", "5", "10", "20", "50", "100", "200", "500", "1000", "2000", "5000" };
        public static readonly string[] POINT_ITEM = { "0", "1", "2", "3", "4" };
        public static readonly string[] UNIT_ITEM = { "kg", "g", "t", "lb", "N", "kN", "mL", "L", "kL", "μV" };
        public static readonly string[] FILTER_MODE_ITEM = { "filter_mode_item_glide_filter" };
        public static readonly string[] FILTER_STRENGTH_ITEM = { "0", "1", "2", "3" };
        public static readonly string[] STABLE_RANGE_ITEM = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static readonly string[] ZERO_TRACK_MODE_ITEM = { "zero_track_mode_item_gross", "zero_track_mode_item_net" };
        public static readonly string[] ZERO_TRACK_RANGE_ITEM = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static readonly string[] ZERO_TRACK_SPEED_ITEM = { "0", "1", "2", "3" };
        public static readonly string[] CREEP_MODE_ITEM = { "creep_mode_item_disable", "creep_mode_item_enable" };
        public static readonly string[] FLICKER_MODE_ITEM = { "flicker_mode_item_disable", "flicker_mode_item_enable" };
        public static readonly string[] COMU_MODE_ITEM = { "comu_mode_item_modbus", "comu_mode_item_customize", "comu_mode_item_yellow_dog", "comu_mode_item_continuous", "comu_mode_item_new_d2_plus" };
        public static readonly string[] COMU_BAUD_ITEM = { "600", "1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "76800", "96000", "115200", "160000", "200000", "250000" };
        public static readonly string[] COMU_PARITY_ITEM = { "comu_parity_item_none", "comu_parity_item_odd", "comu_parity_item_even", "comu_parity_item_space" };
        public static readonly string[] IO_MODE_ITEM = { "io_mode_item_disable", "io_mode_item_input", "io_mode_item_output", "io_mode_item_light" };

        public static string[] GetStringArray(string[] items)
        {
            string[] array = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                array[i] = GetString(items[i]);
                if (string.IsNullOrEmpty(array[i]))
                {
                    array[i] = items[i];
                }
            }
            return array;
        }

        public static string GetString(string name)
        {
            return MultiLang.GetLang(name);
        }

        public static string Port
        {
            get { return Properties.Settings.Default.Port; }
            set { Properties.Settings.Default.Port = value; Properties.Settings.Default.Save(); }
        }

        public static int Baud
        {
            get { return Properties.Settings.Default.Baud; }
            set { Properties.Settings.Default.Baud = value; Properties.Settings.Default.Save(); }
        }

        public static int Parity
        {
            get { return Properties.Settings.Default.Parity; }
            set { Properties.Settings.Default.Parity = value; Properties.Settings.Default.Save(); }
        }

        public static int Addr
        {
            get { return Properties.Settings.Default.Addr; }
            set { Properties.Settings.Default.Addr = value; Properties.Settings.Default.Save(); }
        }

        public static bool Adapt
        {
            get { return Properties.Settings.Default.Adapt; }
            set { Properties.Settings.Default.Adapt = value; Properties.Settings.Default.Save(); }
        }

        public static bool AutoLoad
        {
            get { return Properties.Settings.Default.AutoLoad; }
            set { Properties.Settings.Default.AutoLoad = value; Properties.Settings.Default.Save(); }
        }

        public static bool Tool
        {
            get;
            set;
        }

        public static string Path
        {
            get;
            set;
        }

        public static byte GetAddr()
        {
            return (byte)(Adapt ? 1 : Addr);
        }
    }
}

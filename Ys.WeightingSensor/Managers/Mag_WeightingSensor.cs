using System;

namespace Ys.WeightingSensor_Modbus.Managers
{
    public class Mag_WeightingSensor
    {
        #region 单例
        private static readonly Lazy<Mag_WeightingSensor> instance = new Lazy<Mag_WeightingSensor>(() => new Mag_WeightingSensor());
        private Mag_WeightingSensor() { }
        public static Mag_WeightingSensor Instance
        {
            get
            {
                return instance.Value;
            }
        }
        #endregion



    }
}

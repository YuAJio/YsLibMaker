using AndroidX.Camera.Core;

using System;
using System.Collections.Generic;

namespace Ys.Camera.Droid.Implements {
    public class YsCameraFilter : Java.Lang.Object, ICameraFilter
    {
        private readonly int mId;

        public YsCameraFilter(int mId) : base()
        {
            this.mId = mId;
        }
        public IList<ICameraInfo> Filter(IList<ICameraInfo> p0)
        {
            var result = new List<ICameraInfo>();
            if (mId < p0.Count)
                result.Add(p0[mId]);
            else
                Console.WriteLine($"Camera index {mId} not exist");
            return result;
        }
    }
}

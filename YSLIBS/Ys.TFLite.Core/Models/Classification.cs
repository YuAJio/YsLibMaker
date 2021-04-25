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

namespace Ys.TFLite.Core.Models
{
    public class Classification
    {
        /// <summary>
        /// 相似度
        /// </summary>
        public float Probability { get; set; }
        /// <summary>
        /// 物品名称
        /// </summary>
        public string TagName { get; set; }
    }

}
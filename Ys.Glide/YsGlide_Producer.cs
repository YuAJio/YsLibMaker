using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;

namespace Ys_Glide
{
    /// <summary>
    /// 假装封装一下
    /// </summary>
    public static class YsGlide_Producer
    {

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="iv"></param>
        public static void ProImageToView_Gif(Context context, string url, ImageView iv)
        {
            Glide.With(context).AsGif().Load(url).Apply(RequestOptions.CenterInsideTransform()).Into(iv); ; ;

        }

    }
}
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibUser.Droid.Tools
{
    public class ResourcesTools
    {
        /// <summary>
        /// 获取Assets文件中文本文件中数据
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string ReadAssetsInfoForString(Context context, string filePath)
        {
            if (context == null)
                throw new ArgumentException("context不能为空！");
            if (string.IsNullOrWhiteSpace(filePath))
                return null;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                byte[] buffer = new byte[8192];
                int count = 0;
                //获取流
                var stream = context.Assets.Open(filePath, Android.Content.Res.Access.Streaming);
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        sb.Append(System.Text.Encoding.Default.GetString(buffer, 0, count));
                } while (count > 0);

                var content = sb.ToString();
                return content;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// 文件转换成Byte数组
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] File2Byte(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                var buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);
                return buffur;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

    }
}
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

namespace Ys.BeLazy
{
    /// <summary>
    /// 适配器的基类
    /// メーカー: 萩原真
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class YsBaseApdater<T> : BaseAdapter
    {
        /// <summary>
        /// 适配器数据列表
        /// </summary>
        protected IList<T> list_data;
        /// <summary>
        /// 设置适配器的数据
        /// </summary>
        /// <param name="list_data"></param>
        protected void SetContainerList(IList<T> list_data)
        {
            this.list_data = list_data;
        }
        /// <summary>
        /// 刷新适配器
        /// </summary>
        protected void NotifThisAdapterData()
        {
            this.NotifyDataSetChanged();
        }
        /// <summary>
        /// 适配出来的列表数量
        /// </summary>
        public override int Count
        {
            get
            {
                return list_data == null ? 0 : list_data.Count;
            }
        }
        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }
        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetContentView(position, convertView, parent);
        }
        /// <summary>
        /// 适配内容
        /// </summary>
        /// <param name="position">当前位置</param>
        /// <param name="convertView">上下文</param>
        /// <param name="parent">父布局</param>
        /// <returns></returns>
        protected abstract View GetContentView(int position, View convertView, ViewGroup parent);

        /// <summary>
        /// ItemClick方法获取的数据
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public abstract T this[int position] { get; }

    }
}
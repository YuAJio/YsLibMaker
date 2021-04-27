//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Util;
//using Android.Views;
//using Android.Widget;
//using AndroidX.RecyclerView.Widget;
//using AndroidX.SwipeRefreshLayout.Widget;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Ys.ColorfulGirls.Droid.CustomViews
//{
//    public class RefreshRecyclerView : FrameLayout, SwipeRefreshLayout.IOnRefreshListener
//    {
//        private SwipeRefreshLayout swipeRefresh;
//        private RecyclerView recyclerView;

//        private RecyclerAdapter

//        #region 构造方法
//        public RefreshRecyclerView(Context context) : base(context)
//        {
//        }

//        public RefreshRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
//        {
//        }

//        public RefreshRecyclerView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
//        {
//        }

//        public RefreshRecyclerView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
//        {
//        }

//        protected RefreshRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
//        {
//        }
//        #endregion


//        private void Init(Context context, IAttributeSet attrs = null)
//        {
//            var v = Inflate(context, Resource.Layout.RefreshRecyclerView, this);

//        }


//        public void OnRefresh()
//        {
//            throw new NotImplementedException();
//        }
//    }

//    #region 适配器相关
//    public interface IHandler
//    {
//        void HandMsg(Message message);
//    }

//    public class BaseViewHolder<T> : RecyclerView.ViewHolder
//    {
//        public BaseViewHolder(View itemView) : base(itemView)
//        {
//            OnInitView();
//            itemView.Click -= ItemView_Click; ;
//            itemView.Click += ItemView_Click; ;

//        }

//        private void ItemView_Click(object sender, EventArgs e)
//        {
//            OnItemViewClick(Data);
//        }

//        public void OnInitView()
//        {

//        }


//        public G FindViewById<G>(int resID) where G : View
//        {
//            if (ItemView != null)
//                return ItemView.FindViewById<G>(resID);
//            else return null;
//        }

//        private T dataContent;
//        public T Data
//        {
//            get { return dataContent; }
//            set
//            {
//                if (value == null)
//                    return;
//                dataContent = value;
//            }
//        }

//        public void OnItemViewClick(T data)
//        {

//        }

//    }

//    public class RefreshRecyclerAdapter<T> : RecyclerView.Adapter<BaseViewHolder<T>>, IHandler
//    {
//        public void HandMsg(Message message)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    #endregion

//}
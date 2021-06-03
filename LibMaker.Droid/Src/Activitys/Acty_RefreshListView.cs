using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMaker.Droid.Src.Activitys
{
    [Activity(Label = "Acty_RefreshListView")]
    public class Acty_RefreshListView : AppCompatActivity
    {
        private SwipeRefreshLayout srlRefresh;
        private RecyclerView rvRefresh;

        private Adapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Activity_RefreshList);
            srlRefresh = FindViewById<SwipeRefreshLayout>(Resource.Id.srlRefresh);
            rvRefresh = FindViewById<RecyclerView>(Resource.Id.rvRefresh);

            rvRefresh.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));

            SimListData();

            SetRefreshEvent();
        }

        private void SimListData()
        {
            adapter = new Adapter(this);
            rvRefresh.SetAdapter(adapter);
        }

        private void SetRefreshEvent()
        {
            var reEvent = new OnReFreshHandler();
            reEvent.OnReFreshEvent += OnRefreshEvent;
            srlRefresh.SetOnRefreshListener(reEvent);

        }

        private void OnRefreshEvent(object sender, EventArgs e)
        {
            var list = adapter.DataList;
            list.Add(new Android.Graphics.Color(new System.Random().Next(0, 255), new System.Random().Next(0, 255), new System.Random().Next(0, 255)));
            list.Add(new Android.Graphics.Color(new System.Random().Next(0, 255), new System.Random().Next(0, 255), new System.Random().Next(0, 255)));
            adapter.SetDataList(list.ToList());
        }

        private class OnReFreshHandler : Java.Lang.Object, SwipeRefreshLayout.IOnRefreshListener
        {
            public EventHandler OnReFreshEvent
            { get; set; }

            public void OnRefresh()
            {
                OnReFreshEvent?.Invoke(this, null);
            }

        }

        private class Adapter : Ys.BeLazy.YsBaseRvAdapter<Android.Graphics.Color>
        {
            public Adapter(Context context)
            {
                this.context = context;
            }

            protected override void AbOnBindViewHolder(RecyclerView.ViewHolder vh, int position)
            {
                var holder = vh as ViewHolder;
                if (holder == null)
                    return;
                var item = list_data[position];
                holder.vStock.SetBackgroundColor(item);
            }

            protected override RecyclerView.ViewHolder AbOnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var v = LayoutInflater.From(context).Inflate(Resource.Layout.ListItem_Refresh, parent, false);
                return new ViewHolder(v);
            }

            private class ViewHolder : RecyclerView.ViewHolder
            {
                public View vStock;
                public ViewHolder(View itemView) : base(itemView)
                {
                    vStock = itemView.FindViewById<View>(Resource.Id.vItem);
                }
            }

        }

    }
}
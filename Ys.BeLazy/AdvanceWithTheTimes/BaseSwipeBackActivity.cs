using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace Ys.BeLazy.AdvanceWithTheTimes
{
    public abstract class BaseSwipeBackActivity : YsBaseActivity, SlidingPaneLayout.IPanelSlideListener
    {
        public static string TAG = Java.Lang.Class.FromType(typeof(BaseSwipeBackActivity)).CanonicalName;

        SlidingPaneLayout mSlidingPaneLayout;
        FrameLayout mContainerFl;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            #region 滑动关闭准备工作
            //通过反射来改变SlidingPaneLayout的值
            try
            {
                mSlidingPaneLayout = new SlidingPaneLayout(this);
                //mOverhangSize属性,意思就是做菜单离右边屏幕边缘的距离;
                var f_overHang = Java.Lang.Class.FromType(typeof(SlidingPaneLayout)).GetDeclaredField("mOverhangSize");
                f_overHang.Accessible = true;
                //设置坐菜单离右边屏幕边缘的距离为0,设置全屏
                f_overHang.Set(mSlidingPaneLayout, 0);
                mSlidingPaneLayout.SetPanelSlideListener(this);
                mSlidingPaneLayout.SliderFadeColor = ContextCompat.GetColor(this, Android.Resource.Color.Transparent);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("反射设置SlidingPaneLayout的值时出错 具体错误为:" + e.ToString());
#endif
            }

            //加入两个View,这是左侧菜单.由于Activity是透明的.这里就不用设置了
            mSlidingPaneLayout.AddView(new View(this)
            {
                //设置全屏
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            }, 0);

            //内容布局,用来存放Activity布局用的
            mContainerFl = new FrameLayout(this)
            {
                //设置全屏
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            //内容布局不应该是透明,加了白色背景
            mContainerFl.SetBackgroundColor(Android.Graphics.Color.White);
            mSlidingPaneLayout.AddView(mContainerFl, 1);
            #endregion

            base.OnCreate(savedInstanceState);
        }

        #region 重写设置页面布局逻辑
        public override void SetContentView(int layoutResID)
        {
            SetContentView(LayoutInflater.Inflate(layoutResID, null));
        }

        public override void SetContentView(View view)
        {
            SetContentView(view, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        public override void SetContentView(View view, ViewGroup.LayoutParams @params)
        {
            base.SetContentView(mSlidingPaneLayout, @params);
            mContainerFl.RemoveAllViews();
            mContainerFl.AddView(view, @params);
        }

        #endregion

        #region 侧滑关闭页面接口回调
        public void OnPanelClosed(View panel)
        {
        }

        public void OnPanelOpened(View panel)
        {
            Finish();
            this.OverridePendingTransition(0, Resource.Animation.slide_out_right);
        }

        public void OnPanelSlide(View panel, float slideOffset)
        {
        }
        #endregion

    }
}
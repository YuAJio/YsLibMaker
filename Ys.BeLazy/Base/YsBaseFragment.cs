using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;

namespace Ys.BeLazy.Base
{
    /// <summary>
    /// Fragment基类
    /// </summary>
    public abstract class YsBaseFragment : AndroidX.Fragment.App.Fragment
    {
        private View rootView;// 缓存Fragment view  
        protected YsBaseFragmentActivity YsContext { get; private set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            YsContext = Activity as YsBaseFragmentActivity;
            if (rootView == null)
            {
                rootView = inflater.Inflate(A_GetFragmentContentViewId(), null);
                B_BeforeInitFragmentView();
                C_InitFragmentView(rootView);
                D_InitFragmentData();
            }
            // 缓存的rootView需要判断是否已经被加过parent，如果有parent需要从parent删除，要不然会发生这个rootview已经有parent的错误。  
            ViewGroup parent = (ViewGroup)rootView.Parent;
            if (parent != null)
            {
                parent.RemoveView(rootView);
            }
            return rootView;
        }

        private bool IsActivityUseable()
        {
            if ((Activity == null) || Activity.IsFinishing || Activity.IsRestricted)
                return false;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
            {
                try
                {
                    if (Activity.IsDestroyed)
                        return false;

                }
                catch (Exception e)
                {
                }
            }
            return true;
        }

        #region 抽象封装类
        /// <summary>
        /// 获取依赖布局
        /// </summary>
        /// <returns></returns>
        public abstract int A_GetFragmentContentViewId();
        /// <summary>
        /// 在初始化布局前处理的代码
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public abstract void B_BeforeInitFragmentView();
        /// <summary>
        /// 初始化布局
        /// </summary>
        public abstract void C_InitFragmentView(View view);
        /// <summary>
        /// 初始化代码
        /// </summary>
        public abstract void D_InitFragmentData();
        /// <summary>
        /// 初始化点击
        /// </summary>
        /// <param name="view"></param>
        /// <param name="e"></param>
        public abstract void E_SetOnFragmentClick(View view, EventArgs e);
        #endregion

        #region 封装方法
        /// <summary>
        /// 绑定点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnClickListener(object sender, EventArgs e)
        {
            var v = sender as View;
            E_SetOnFragmentClick(v, e);
        }

        #region 工具方法
        /// <summary>
        /// 省去view.FInd....懒惰才是最好的! 吊·不懒惰!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        protected T FindViewById<T>(int id) where T : View
        {
            return rootView?.FindViewById<T>(id);
        }
        #endregion

        #region Toast提示框
        protected void ShowMsgShort(string msg)
        {
            Toast.MakeText(YsContext, msg, ToastLength.Short).Show();

        }
        protected void ShowMsgLong(string msg)
        {
            Toast.MakeText(YsContext, msg, ToastLength.Long).Show();
        }
        #endregion

        #region 显示隐藏控件
        protected enum CoverFlag
        {
            Gone = 0,
            Visible = 1,
            Invisible = 2
        }
        protected void CoverUIControl(CoverFlag coverFlag, params View[] v)
        {
            foreach (var item in v)
            {
                if (item == null)
                    continue;
                switch (coverFlag)
                {
                    case CoverFlag.Gone:
                        item.Visibility = ViewStates.Gone;
                        break;
                    case CoverFlag.Visible:
                        item.Visibility = ViewStates.Visible;
                        break;
                    case CoverFlag.Invisible:
                        item.Visibility = ViewStates.Invisible;
                        break;
                }
            }
        }
        /// <summary>
        ///显示隐藏控件
        /// </summary>
        /// <param name="flag">0.隐藏    1.显示   2.隐藏并占位</param>
        /// <param name="v">View们</param>
        protected void CoverUIControl(int flag, params View[] v)
        {
            foreach (var item in v)
            {
                if (item == null)
                    continue;
                switch (flag)
                {
                    case 0:
                        item.Visibility = ViewStates.Gone;
                        break;
                    case 1:
                        item.Visibility = ViewStates.Visible;
                        break;
                    case 2:
                        item.Visibility = ViewStates.Invisible;
                        break;
                }
            }
        }
        #endregion

        #region 权限相关
        /// <summary>
        /// 用特殊办法检查权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        protected bool IsPermissionGranted(string permission)
        {
            bool result = true;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                try
                {
                    var info = YsContext.PackageManager.GetPackageInfo(YsContext.PackageName, 0);
                    var targetSdkVersion = info.ApplicationInfo.TargetSdkVersion;
                    if (targetSdkVersion >= BuildVersionCodes.M)
                        result = YsContext.CheckSelfPermission(permission) == Permission.Granted;
                    else
                        result = PermissionChecker.CheckSelfPermission(YsContext, permission) == PermissionChecker.PermissionGranted;
                }
                catch (PackageManager.NameNotFoundException e)
                {

                }
            }
            return result;
        }

        #endregion

        #endregion
    }

}
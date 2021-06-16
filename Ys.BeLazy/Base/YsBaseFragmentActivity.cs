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
using AndroidX.Fragment.App;

namespace Ys.BeLazy.Base
{
    /// <summary>
    /// FragmentActivity的基类
    /// </summary>
    public abstract class YsBaseFragmentActivity :FragmentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (A_GetContentViewId() <= 0)
                SetContentView(new LinearLayout(this));
            else
                SetContentView(A_GetContentViewId());

            B_BeforeInitView();
            C_InitView();
            D_BindEvent();
            E_InitData();

        }

        #region 抽象类
        /// <summary>
        /// Activity界面给予
        /// </summary>
        /// <returns></returns>
        public abstract int A_GetContentViewId();
        /// <summary>
        /// 在初始化控件前所做操作
        /// </summary>
        public abstract void B_BeforeInitView();
        /// <summary>
        /// 初始化控件操作
        /// </summary>
        public abstract void C_InitView();
        /// <summary>
        /// 绑定控件时间操作
        /// </summary>
        public abstract void D_BindEvent();
        /// <summary>
        /// 初始化数据操作
        /// </summary>
        public abstract void E_InitData();
        /// <summary>
        /// 设置点击事件操作
        /// </summary>
        /// <param name="v"></param>
        /// <param name="e"></param>
        public abstract void F_OnClickListener(View v, EventArgs e);
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
            F_OnClickListener(v, e);
        }

        #region 弹出框相关
        /// <summary>
        /// YsDialog宿主
        /// </summary>
        private Dialog ysDialogHost;

        #region 提示框
        /// <summary>
        /// 显示Android提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">内容消息</param>
        /// <param name="onSureClick">确定事件</param>
        /// <param name="onCancelClick">取消事件</param>
        /// <param name="sureText">确定文字</param>
        /// <param name="cancelText">取消文字</param>
        public void ShowAndroidPromptBox(string title, string msg, Action onSureClick, Action onCancelClick, string sureText = "确定", string cancelText = "取消")
        {
        }

        /// <summary>
        /// 显示IOS提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">内容消息</param>
        /// <param name="onSureClick">确定事件</param>
        /// <param name="onCancelClick">取消事件</param>
        /// <param name="sureText">确定文字</param>
        /// <param name="cancelText">取消文字</param>
        public void ShowIOSAndroidPromptBos(string title, string msg, Action onSureClick, Action onCancelClick, string sureText = "确定", string cancelText = "取消")
        {
        }

        #endregion

        #region 等待框
        /// <summary>
        /// 显示小小等待框
        /// </summary>
        /// <param name="msg">等待信息</param>
        /// <param name="isCanCancel">是否可取消</param>
        /// <param name="isOutSideTouch">是否可外部点击取消</param>
        public void ShowWaitDialog_Samll(string msg, bool isCanCancel = false, bool isOutSideTouch = false, bool isretry = true)
        {
        }

        /// <summary>
        /// 显示正常等待框
        /// </summary>
        /// <param name="msg">等待信息</param>
        /// <param name="colorRes">提示字体颜色资源</param>
        /// <param name="isCanCancel">是否可取消</param>
        /// <param name="isOutSideTouch">是否可外部点击取消</param>
        public void ShowWaitDialog_Normal(string msg, int colorRes = -1, bool isCanCancel = false, bool isOutSideTouch = false)
        {
        }
        public void HideWaitDiaLog()
        {
        }
        #endregion

        #endregion

        #region Toast提示框
        protected void ShowMsgShort(string msg)
        {
            Toast.MakeText(this, msg, ToastLength.Short).Show();

        }
        protected void ShowMsgLong(string msg)
        {
            Toast.MakeText(this, msg, ToastLength.Long).Show();
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

        #region 隐藏软键盘
        private bool IsShouldHideKeyBoard(View v, MotionEvent ev)
        {
            if (v != null && v is EditText)
            {
                int[] l = { 0, 0 };
                v.GetLocationInWindow(l);
                var left = l[0];
                var top = l[1];
                var bottom = top + v.Height;
                var right = left + v.Width;
                if (ev.GetX() > left && ev.GetX() < right && ev.GetY() > top && ev.GetY() < bottom)
                    return false;
                else
                    return true;
            }
            return false;
        }

        protected void HideKeyBorad(IBinder token)
        {
            if (token != null)
            {
                var inputMethodManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.NotAlways);
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                var v = CurrentFocus;
                if (IsShouldHideKeyBoard(v, ev))
                    this.HideKeyBorad(v.WindowToken);
            }

            return base.DispatchTouchEvent(ev);
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
                    var info = this.PackageManager.GetPackageInfo(this.PackageName, 0);
                    var targetSdkVersion = info.ApplicationInfo.TargetSdkVersion;
                    if (targetSdkVersion >= BuildVersionCodes.M)
                        result = this.CheckSelfPermission(permission) == Permission.Granted;
                    else
                        result = PermissionChecker.CheckSelfPermission(this, permission) == PermissionChecker.PermissionGranted;
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
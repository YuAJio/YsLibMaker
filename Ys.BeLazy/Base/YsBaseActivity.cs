using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Yurishi.Ysdialog;
using IMAS_YsDialog.Listener;

namespace Ys.BeLazy
{
    /// <summary>
    /// Activity的基类
    /// </summary>
    public abstract class YsBaseActivity : AppCompatActivity
    {
        #region Activity进入退出动画
        protected int activityCloseEnterAnimation;
        protected int activityCloseExitAnimaiton;
        #endregion

        protected Bundle SaveInstanceState { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SaveInstanceState = savedInstanceState;
            #region 动画效果
            var activityStyle = Theme.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.WindowAnimationStyle });
            var windowAnimationStyleResid = activityStyle.GetResourceId(0, 0);
            activityStyle.Recycle();
            activityStyle = Theme.ObtainStyledAttributes(windowAnimationStyleResid, new int[] { Android.Resource.Attribute.ActivityCloseEnterAnimation, Android.Resource.Attribute.ActivityCloseExitAnimation });
            activityCloseEnterAnimation = activityStyle.GetResourceId(0, 0);
            activityCloseExitAnimaiton = activityStyle.GetResourceId(1, 0);
            activityStyle.Recycle();
            OverridePendingTransition(activityCloseEnterAnimation, activityCloseExitAnimaiton);
            #endregion

            if (A_GetContentViewId() <= 0)
                this.SetContentView(new LinearLayout(this));
            else
                this.SetContentView(A_GetContentViewId());

            B_BeforeInitView();
            C_InitView();
            D_BindEvent();
            E_InitData();
        }

        #region 初始化相关
        /// <summary>
        /// 初始化YsDialog
        /// </summary>
        private void InitYsDialog()
        {
            YsDialogManager.Init(this);
        }
        #endregion

        #region 抽象方法
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
        protected void ShowAndroidPromptBox(string title, string msg, Action onSureClick, Action onCancelClick, string sureText = "确定", string cancelText = "取消")
        {
            YsDialogManager.BuildMdAlert(title, msg, new YsMyDialogListener(onSureClick, onCancelClick)).SetBtnText(sureText, cancelText).Show();
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
        protected void ShowIOSAndroidPromptBos(string title, string msg, Action onSureClick, Action onCancelClick, string sureText = "确定", string cancelText = "取消")
        {

            YsDialogManager.BuildIosAlert(title, msg, new YsMyDialogListener(onSureClick, onCancelClick)).SetBtnText(sureText, cancelText).Show();
        }

        #endregion

        #region 等待框
        /// <summary>
        /// 显示小小等待框
        /// </summary>
        /// <param name="msg">等待信息</param>
        /// <param name="isCanCancel">是否可取消</param>
        /// <param name="isOutSideTouch">是否可外部点击取消</param>
        protected void ShowWaitDialog_Samll(string msg, bool isCanCancel = false, bool isOutSideTouch = false, bool isretry = true)
        {
            try
            {
                ysDialogHost = YsDialogManager.BuildLoading(msg).SetCancelable(isCanCancel, isOutSideTouch).Show();
            }
            catch (Exception e)
            {
                if (isretry)
                {
                    InitYsDialog();
                    ShowWaitDialog_Samll(msg, isCanCancel, isOutSideTouch, isretry: false);
                }

            }
        }

        /// <summary>
        /// 显示正常等待框
        /// </summary>
        /// <param name="msg">等待信息</param>
        /// <param name="colorRes">提示字体颜色资源</param>
        /// <param name="isCanCancel">是否可取消</param>
        /// <param name="isOutSideTouch">是否可外部点击取消</param>
        protected void ShowWaitDialog_Normal(string msg, int colorRes = -1, bool isCanCancel = false, bool isOutSideTouch = false)
        {
            if (colorRes > 0)
                ysDialogHost = YsDialogManager.BuildMdLoading(msg).SetCancelable(isCanCancel, isOutSideTouch).SetMsgColor(colorRes).Show();
            else
                ysDialogHost = YsDialogManager.BuildMdLoading(msg).SetCancelable(isCanCancel, isOutSideTouch).Show();
        }
        protected void HideWaitDiaLog()
        {
            YsDialogManager.Dismiss(ysDialogHost);
        }
        #endregion

        #region 单选框
        /// <summary>
        /// 显示单选框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="strings"></param>
        /// <param name="onItemSelectAct"></param>
        protected void ShowIOSSingleSelectDialog(List<string> strings, Action<string, int> onItemSelectAct)
        {
            try
            {
                var jk = YsDialogManager.BuildIosSingleChoose(strings, new YsMyItemDialogListener(onItemSelectAct));
                jk.ItemTxtSize = 48;
                jk.Show();
            }
            catch (Exception e)
            {
                ShowIOSSingleSelectDialog(strings, onItemSelectAct);
            }

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
        /// <summary>
        /// 隐藏软键盘
        /// </summary>
        protected void HideTheSoftKeybow(EditText et = null)
        {
            var inputMethodManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            if (inputMethodManager.IsActive)
                inputMethodManager.HideSoftInputFromWindow(et.WindowToken, 0);
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

        #region 生命周期
        protected override void OnResume()
        {
            base.OnResume();
            InitYsDialog();
        }
        #endregion
    }
}
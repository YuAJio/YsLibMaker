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
using Com.Yurishi.Ysdialog.Dialog.Interfaces;

namespace IMAS_YsDialog.Listener
{
    /// <summary>
    /// Dialog按钮点击监听
    /// </summary>
    public class YsMyDialogListener : MyDialogListener
    {

        /// <summary>
        /// Dialog按钮点击监听
        /// </summary>
        /// <param name="onFirstAct">第一个按钮</param>
        /// <param name="onSecondAct">第二个按钮</param>
        /// <param name="onThirdAct">第三个按钮</param>
        /// <param name="onCancelAct">取消按钮</param>
        public YsMyDialogListener(Action onFirstAct, Action onSecondAct, Action onThirdAct, Action onCancelAct)
        {
            this.OnFirstAct = onFirstAct;
            this.OnSecondAct = onSecondAct;
            this.OnThirdAct = onThirdAct;
            this.OnCancelAct = onCancelAct;
        }
        public YsMyDialogListener(Action onFirstAct, Action onSecondAct, Action onCancelAct)
        {
            this.OnFirstAct = onFirstAct;
            this.OnSecondAct = onSecondAct;
            this.OnCancelAct = onCancelAct;
        }
        public YsMyDialogListener(Action onFirstAct, Action onCancelAct)
        {
            this.OnFirstAct = onFirstAct;
            this.OnCancelAct = onCancelAct;
        }
        public YsMyDialogListener(Action onFirstAct)
        {
            this.OnFirstAct = onFirstAct;
        }

        public YsMyDialogListener() { }

        public Action OnFirstAct { get; private set; }
        public Action OnSecondAct { get; private set; }
        public Action OnThirdAct { get; private set; }
        public Action OnCancelAct { get; private set; }

        public override void OnFirst()
        {
            OnFirstAct?.Invoke();
        }

        public override void OnSecond()
        {
            OnSecondAct?.Invoke();
        }

        public override void OnThird()
        {
            if (OnThirdAct == null)
                base.OnThird();
            else
                OnThirdAct?.Invoke();
        }

        public override void OnCancle()
        {
            if (OnCancelAct == null)
                base.OnCancle();
            else
                OnCancelAct?.Invoke();
        }
    }
}
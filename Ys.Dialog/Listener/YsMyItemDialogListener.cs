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
using Java.Lang;

namespace IMAS_YsDialog.Listener
{
    /// <summary>
    /// Dialog多选监听器
    /// </summary>
    public class YsMyItemDialogListener : MyItemDialogListener
    {
        /// <summary>
        /// Dialog多选监听器
        /// </summary>
        /// <param name="onItemClickAct">Item点击事件</param>
        /// <param name="onBottomBtnClickAct">按钮点击事件</param>
        public YsMyItemDialogListener(Action<string, int> onItemClickAct, Action onBottomBtnClickAct = null)
        {
            this.OnItemClickAct = onItemClickAct;
            this.OnBottomBtnClickAct = onBottomBtnClickAct;
        }

        public Action<string, int> OnItemClickAct { get; private set; }
        public Action OnBottomBtnClickAct { get; private set; }

        public override void OnItemClick(ICharSequence msg, int position)
        {
            OnItemClickAct?.Invoke(msg.ToString(), position);
        }

        public override void OnBottomBtnClick()
        {
            if (OnBottomBtnClickAct == null)
                base.OnBottomBtnClick();
            else
                OnBottomBtnClickAct?.Invoke();


        }
    }
}
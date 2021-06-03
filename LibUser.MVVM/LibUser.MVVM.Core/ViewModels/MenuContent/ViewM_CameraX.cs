using MvvmCross.Commands;
using MvvmCross.ViewModels;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LibUser.MVVM.Core.ViewModels.MenuContent
{
    public class ViewM_CameraX : MvxViewModel
    {
        #region 声明主动触发View的Action
        public Action ViewAct_PageClose;
        #endregion

        public ICommand Click_EventTrigger => _eventTriggerCommand ?? (_eventTriggerCommand = new MvxCommand(EventTriggerClickEvent));
        private ICommand _eventTriggerCommand;
        private void EventTriggerClickEvent()
        {
            ViewAct_PageClose?.Invoke();
        }

    }
}

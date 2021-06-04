using MvvmCross.ViewModels;

using System;
using System.Collections.Generic;
using System.Text;

namespace LibUser.MVVM.Core.ViewModels.MenuContent
{
    /// <summary>
    /// 蓝牙页面ViewModel
    /// </summary>
    public class ViewM_BlueTooth : MvxViewModel
    {
        private bool VisibleBo_InputArea { get { return bo_InputArea; } set { bo_InputArea = value; RaisePropertyChanged(() => VisibleBo_InputArea); } }
        private bool VisibleBo_OutputArea { get { return bo_OutputArea; } set { bo_OutputArea = value; RaisePropertyChanged(() => VisibleBo_OutputArea); } }
        private bool bo_InputArea;
        private bool bo_OutputArea;

        private string Content_Input
        {
            get { return _contentInput; }
            set
            {
                VisibleBo_InputArea = !string.IsNullOrEmpty(value);
                _contentInput = value;
                RaisePropertyChanged(() => Content_Input);
            }
        }
        private string Content_Output
        {
            get { return _contentOutput; }
            set
            {
                VisibleBo_OutputArea = !string.IsNullOrEmpty(value);
                _contentOutput = value;
                RaisePropertyChanged(() => Content_Output);
            }
        }
        private string _contentInput;
        private string _contentOutput;


    }

}

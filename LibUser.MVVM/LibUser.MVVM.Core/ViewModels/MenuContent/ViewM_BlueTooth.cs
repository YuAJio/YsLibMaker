using LibUser.MVVM.Core.Proxys;

using MvvmCross.Commands;
using MvvmCross.ViewModels;


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LibUser.MVVM.Core.ViewModels.MenuContent
{
    /// <summary>
    /// 蓝牙页面ViewModel
    /// </summary>
    public class ViewM_BlueTooth : MvxViewModel
    {
        #region 暂时未跨平台的需要各个平台独自处理的业务回调
        public Action ActionEvent_ScanBlueTooth;
        #endregion

        #region 文字相关
        public bool VisibleBo_InputArea { get { return bo_InputArea; } set { bo_InputArea = value; RaisePropertyChanged(() => VisibleBo_InputArea); } }
        public bool VisibleBo_OutputArea { get { return bo_OutputArea; } set { bo_OutputArea = value; RaisePropertyChanged(() => VisibleBo_OutputArea); } }
        private bool bo_InputArea;
        private bool bo_OutputArea;

        public string Content_Input
        {
            get { return _contentInput; }
            set
            {
                VisibleBo_InputArea = !string.IsNullOrEmpty(value);
                _contentInput = value;
                RaisePropertyChanged(() => Content_Input);
            }
        }
        public string Content_Output
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
        #endregion

        #region 事件相关
        private ICommand _scanClickCommand;
        public ICommand ClickEvent_ScanBluetooth => _scanClickCommand ??= new MvxCommand(EventJK_JKLIOU);
        private void EventJK_JKLIOU()
        {//点击了扫描
            ActionEvent_ScanBlueTooth?.Invoke();
        }

        private ICommand _sendContentClickCommand;
        public ICommand ClickEvent_SendContent => _sendContentClickCommand ??= new MvxCommand(() =>
        {//点击了发送消息

        });

        #endregion

        #region 列表相关
        private ObservableCollection<Models.Mod_BlueTooth> _menuList;
        public ObservableCollection<Models.Mod_BlueTooth> List_Bluetooth
        {
            get
            {
                return _menuList;
            }
            set
            {
                _menuList = value;
                RaisePropertyChanged(() => List_Bluetooth);
            }
        }

        public void ListAdd_Bluetooth(Models.Mod_BlueTooth item)
        {
            if (List_Bluetooth == null)
                List_Bluetooth = new List<Models.Mod_BlueTooth> { item }.ToObservableCollection();
            if (List_Bluetooth.Any(x => x.BtCode == item.BtCode)) return;
            List_Bluetooth.AddOb(item);
        }
        #endregion

    }

}

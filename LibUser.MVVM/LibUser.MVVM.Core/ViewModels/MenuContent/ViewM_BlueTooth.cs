using MvvmCross.Commands;
using MvvmCross.ViewModels;

using Plugin.BluetoothLE;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace LibUser.MVVM.Core.ViewModels.MenuContent
{
    /// <summary>
    /// 蓝牙页面ViewModel
    /// </summary>
    public class ViewM_BlueTooth : MvxViewModel
    {
        #region 文字相关
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
        #endregion

        #region 事件相关
        private ICommand _scanClickCommand;
        private ICommand ClickEvent_ScanBluetooth => _scanClickCommand ??= new MvxCommand(() =>
        {//点击了扫描
            CrossBleAdapter.Current.Scan().Subscribe(jk =>
            {
                List_Bluetooth.Add(new Models.Mod_BlueTooth
                {
                    BtName = jk.Device?.Name,
                    BtCode = jk.Device.Uuid.ToString(),
                    BtConnected = jk.Device.Status == ConnectionStatus.Connected
                });
            });
        });

        private ICommand _sendContentClickCommand;
        private ICommand ClickEvent_SendContent => _sendContentClickCommand ??= new MvxCommand(() =>
        {//点击了发送消息

        });

        #endregion

        #region 列表相关
        private ObservableCollection<Models.Mod_BlueTooth> _menuList;
        private ObservableCollection<Models.Mod_BlueTooth> List_Bluetooth
        {
            get
            {
                if (_menuList == null)
                    _menuList = new ObservableCollection<Models.Mod_BlueTooth>();
                return _menuList;
            }
            set
            {
                _menuList = value;
                RaisePropertyChanged(() => List_Bluetooth);
            }
        }

        #endregion

    }

}

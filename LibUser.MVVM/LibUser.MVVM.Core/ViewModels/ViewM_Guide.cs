using LibUser.MVVM.Core.Proxys;

using MvvmCross.Commands;
using MvvmCross.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace LibUser.MVVM.Core.ViewModels
{
    public class ViewM_Guide : MvxViewModel
    {
        #region 声明主动触发View的Action
        public Action<Type> ViewAct_GotoNewPage;
        #endregion

        #region 列表处理
        private ObservableCollection<Models.Mod_GuildMenu> _menuList;
        public ObservableCollection<Models.Mod_GuildMenu> List_Menu
        {
            get
            {
                return _menuList;
            }
            set
            {
                _menuList = value;
                RaisePropertyChanged(() => List_Menu);
            }
        }

        #endregion

        #region 控件事件处理
        public ICommand MenuClickCommand
        {
            get
            {
                return new MvxCommand<Models.Mod_GuildMenu>(menu =>
                {
                    ViewAct_GotoNewPage?.Invoke(menu.ViewType);
                });
            }
        }
        #endregion

        #region 一般事务
        public void InitAndCreateMenu(List<Models.Mod_GuildMenu> mod_GuildMenus)
        {
            List_Menu.AddRange(mod_GuildMenus);
        }
        #endregion
    }
}

using LibUser.MVVM.Core.ViewModels;

using MvvmCross.ViewModels;

using System;
using System.Collections.Generic;
using System.Text;

namespace LibUser.MVVM.Core
{
    public class CommandCenter : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<ViewM_Guide>();
        }
    }
}

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
using Prism;
using Prism.Ioc;
using YS_BTPrint;

namespace LibMaker
{
    [Application]
    public class Ys_Application : Application, IPlatformInitializer
    {
        public Ys_Application()
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IBluetoothService>(new BluetoothService());
        }
    }
}
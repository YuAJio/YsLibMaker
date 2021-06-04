using MvvmCross.Commands;
using MvvmCross.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibUser.MVVM.Core.ViewModels.MenuContent
{
    public class ViewM_CameraX : MvxViewModel
    {
        public string FilePath_OSRootPath { set; private get; }

        #region 声明主动触发View的Action
        public Action ViewAct_PageClose;
        public Action ViewAct_EventTriggerLongClick;
        #endregion


        private string _eventContent;
        public string EventContent
        {
            get { return _eventContent; }
            set
            {
                _eventContent = value;
                RaisePropertyChanged(() => EventContent);
            }
        }

        #region 控件事件
        public ICommand ClickEvent_EventTrigger => _eventTriggerCommand ??= new MvxCommand(EventTriggerClickEvent);
        private ICommand _eventTriggerCommand;
        private void EventTriggerClickEvent()
        {
            LogicLock_AllowFlameClassify = !LogicLock_AllowFlameClassify;
        }

        public ICommand LongClickEvent_EventTrigger => _eventTriggerLongClickCommand ??= new MvxCommand(EventTriggerLongClickEvent);
        private ICommand _eventTriggerLongClickCommand;
        private void EventTriggerLongClickEvent()
        {
            ViewAct_EventTriggerLongClick?.Invoke();
        }

        public ICommand ClickEvent_PageFinish => _eventPageFinishCommand ??= new MvxCommand(PageFinishClickEvent);
        private ICommand _eventPageFinishCommand;
        private void PageFinishClickEvent()
        {
            ViewAct_PageClose?.Invoke();
        }

        #endregion

        #region TFLite分类相关
        /// <summary>
        /// 食材代码转换食材名称
        /// </summary>
        private List<Lable2Mat> List_Lable2Mat;

        public EventHandler<byte[]> EventHandler_Classify;
        public void SetLable2MatList(string resJson)
        {
            List_Lable2Mat = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Lable2Mat>>(resJson);
        }

        public bool LogicLock_AllowFlameClassify { get; private set; } = false;
        /// <summary>
        /// 逻辑锁,是否正在执行识别
        /// </summary>
        private bool LogicLock_IsClassifing;

        public void ClassifycationStart(byte[] picStream)
        {
            if (LogicLock_IsClassifing)
                Console.WriteLine("Classify Thread Is Busy");
            else
            {
                LogicLock_IsClassifing = true;
                EventHandler_Classify?.Invoke(this, picStream);
            }
        }
        public void ClassificationCompleted(List<ClassifyResult> classifyResults)
        {
            LogicLock_IsClassifing = false;
            if (classifyResults != null && classifyResults.Any())
            {
                var result = (from j in classifyResults join k in List_Lable2Mat on j.Tag equals k.MatCode select new { j.Percent, k.MatName }).OrderByDescending(x => x.Percent).ToList();
                EventContent = $"" +
                $"识别结果前三为:<{result[0]?.MatName}/{result[1]?.MatName}/{result[2]?.MatName}>" +
                $"\n识别精度分别为:<{result[0]?.Percent:N2}/{result[1]?.Percent:N2}/{result[2]?.Percent:N2}>";
            }
        }
        #endregion



        #region 实体类
        private class Lable2Mat
        {
            /// <summary>
            /// 食材名称
            /// </summary>
            public string MatName { get; set; }
            /// <summary>
            /// 食材代码
            /// </summary>
            public string MatCode { get; set; }
        }
        public class ClassifyResult
        {
            public float Percent { get; set; }
            public string Tag { get; set; }
        }
        #endregion
    }
}

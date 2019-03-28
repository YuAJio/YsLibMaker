using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;

namespace Ys.QuartzStuff
{
    public class QS_JobManager
    {
        private IScheduler schedudler;
        public QS_JobManager()
        {
            schedudler = StdSchedulerFactory.GetDefaultScheduler().Result;
            schedudler.Start();
        }

        public void AddJob<T>(int second) where T : QS_JobBase
        {
            var jk = Activator.CreateInstance<T>();
            IDictionary<string, object> jdData = new Dictionary<string, object>
            {
                { "name", jk }
            };
            var job1 = JobBuilder.Create<QS_JobInstance>().SetJobData(new JobDataMap(jdData)).Build();
            var trigger1 = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(second).RepeatForever()).Build();

            schedudler.ScheduleJob(job1, trigger1);

        }

        public void AddJob<T>(string rule) where T : QS_JobBase
        {
            var jbInstance = Activator.CreateInstance<T>();
            IDictionary<string, object> jbData = new Dictionary<string, object>();
            jbData.Add("name", jbInstance);

            IJobDetail job1 = JobBuilder.Create<QS_JobInstance>()
                .SetJobData(new JobDataMap(jbData)).Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule(rule).Build();

            schedudler.ScheduleJob(job1, trigger1);

        }

    }
}
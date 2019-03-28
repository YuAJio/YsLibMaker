using System;
using System.Threading.Tasks;
using Quartz;

namespace Ys.QuartzStuff
{
    public class QS_JobInstance : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            var jobs = context.JobDetail.JobDataMap;
            if (jobs != null)
            {
                foreach (var item in jobs.Values)
                {
                    try
                    {
                        var jobObj = item as QS_JobBase;
                        jobObj.Run();
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }


    }
}
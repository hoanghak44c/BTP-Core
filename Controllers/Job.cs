using System;
using System.Threading;

namespace QLBH.Core.Controllers
{
    internal class Job
    {
        private readonly Thread innerThead;

        internal Job(Delegate job)
        {
            innerThead = new Thread(Delegate.CreateDelegate(typeof(ThreadStart), job.Target, job.Method) as ThreadStart);
        }

        internal static Job CreateJob(Delegate job)
        {
            return new Job(job);
        }

        internal void DoASyncWork()
        {
            innerThead.Start();
        }

        internal void DoSyncWork()
        {
            innerThead.Start();

            innerThead.Join();
        }

        internal static void Delay(int miliseconds)
        {
            Thread.CurrentThread.Join(miliseconds);
        }

    }
}
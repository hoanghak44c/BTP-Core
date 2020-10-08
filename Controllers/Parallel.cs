using QLBH.Core.Interfaces;

namespace QLBH.Core.Controllers
{
    public abstract class Parallel
    {
        protected void Delay(int miliseconds)
        {
            Job.Delay(miliseconds);
        }

        protected void DoASyncWork(UnitWorkDelegate job)
        {
            Job.CreateJob(job).DoASyncWork();
        }

        protected void DoSyncWork(UnitWorkDelegate job)
        {
            Job.CreateJob(job).DoSyncWork();
        }
    }
}
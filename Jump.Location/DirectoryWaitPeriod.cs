using System;

namespace Jump.Location
{
    class DirectoryWaitPeriod
    {
        private readonly IRecord record;
        private readonly DateTime startTime;
        private bool hasDestroyed;

        public DirectoryWaitPeriod(IRecord record, DateTime now)
        {
            this.record = record;
            startTime = now;
        }

        public void CloseAndUpdate()
        {
            if (hasDestroyed) throw new InvalidOperationException("You can't call CloseAndUpdate multiple times");

            record.AddTimeSpent(DateTime.Now - startTime);
            hasDestroyed = true;
        }
    }
}
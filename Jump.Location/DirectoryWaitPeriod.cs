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
            if (hasDestroyed)
            {
                // ignore, don't throw error messages.
                return;
            }

            record.AddTimeSpent(DateTime.Now - startTime);
            hasDestroyed = true;
        }
    }
}
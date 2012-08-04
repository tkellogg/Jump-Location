using System;

namespace Jump.Location
{
    class DirectoryWaitPeriod
    {
        private readonly Record record;
        private readonly DateTime startTime;
        private bool hasDestroyed;

        public DirectoryWaitPeriod(Record record, DateTime now)
        {
            this.record = record;
            startTime = now;
        }

        public void CloseAndUpdate()
        {
            if (hasDestroyed) throw new InvalidOperationException("You can't call CloseAndUpdate multiple times");

            var seconds = (DateTime.Now - startTime).TotalSeconds;
            record.Weight += (decimal) seconds;

            hasDestroyed = true;
        }
    }
}
using System;
using Xunit;
using Should;

namespace Jump.Location.Specs
{
    public class DirectoryWaitPeriodSpec
    {
        [Fact]
        public void It_updates_the_records_weight_upon_CloseAndUpdate()
        {
            var now = DateTime.Now.AddMinutes(-3);
            var record = new Record("x::y", 0);
            var waitPeriod = new DirectoryWaitPeriod(record, now);

            waitPeriod.CloseAndUpdate();

            Assert.True(record.Weight > 0);
        }

        [Fact]
        public void You_cant_call_CloseAndUpdate_twice()
        {
            var waitPeriod = new DirectoryWaitPeriod(new Record("x::y", 0), DateTime.Now);
            waitPeriod.CloseAndUpdate();
            Assert.Throws<InvalidOperationException>(() => waitPeriod.CloseAndUpdate());
        }
    }
}

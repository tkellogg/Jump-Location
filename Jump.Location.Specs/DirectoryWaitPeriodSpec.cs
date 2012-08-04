using System;
using Moq;
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
            var recordMock = new Mock<IRecord>();
            var waitPeriod = new DirectoryWaitPeriod(recordMock.Object, now);

            waitPeriod.CloseAndUpdate();
            recordMock.Verify(x => x.AddTimeSpent(It.IsAny<TimeSpan>()));
        }

        [Fact]
        public void You_cant_call_CloseAndUpdate_twice()
        {
            var waitPeriod = new DirectoryWaitPeriod(Mock.Of<IRecord>(), DateTime.Now);
            waitPeriod.CloseAndUpdate();
            Assert.Throws<InvalidOperationException>(() => waitPeriod.CloseAndUpdate());
        }
    }
}

using System;
using System.Threading;
using Moq;
using Xunit;

namespace Jump.Location.Specs
{
    public class CommandControllerSpec
    {
        public class DescribeUpdateTimes
        {
            private readonly Mock<IDatabase> dbMock;
            private readonly Mock<IFileStoreProvider> fsMock;
            private readonly CommandController controller;

            public DescribeUpdateTimes()
            {
                dbMock = new Mock<IDatabase>();
                fsMock = new Mock<IFileStoreProvider>();
                controller = new CommandController(dbMock.Object, fsMock.Object);
            }

            [Fact]
            public void It_saves_eventually()
            {
                var recordMock = new Mock<IRecord>();
                recordMock.SetupAllProperties();
                dbMock.Setup(x => x.GetByFullName(It.IsAny<string>())).Returns(recordMock.Object);

                controller.UpdateLocation("foo");
                controller.UpdateLocation("bar");
                Thread.Sleep(30);
                fsMock.Verify(x => x.Save(dbMock.Object));
            }

            [Fact]
            public void It_updates_weights()
            {
                var recordMock = new Mock<IRecord>();
                recordMock.SetupAllProperties();
                dbMock.Setup(x => x.GetByFullName("foo")).Returns(recordMock.Object);

                controller.UpdateLocation("foo");
                Thread.Sleep(30);
                controller.UpdateLocation("foo");

                recordMock.Verify(x => x.AddTimeSpent(It.IsAny<TimeSpan>()), Times.Once());
            }
        }
    }
}

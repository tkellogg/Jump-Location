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
                controller.UpdateLocation("foo");
                Thread.Sleep(30);
                fsMock.Verify(x => x.Save(dbMock.Object));
            }
        }
    }
}

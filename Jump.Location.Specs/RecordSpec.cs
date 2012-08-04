using System;
using Should;
using Xunit;
using Xunit.Extensions;

namespace Jump.Location.Specs
{
    public class RecordSpec
    {
        public class DescribeFullName
        {
            [Fact]
            public void It_splits_the_supplied_path_on_the_cons()
            {
                var record = new Record("provider::path", 0);
                record.Provider.ShouldEqual("provider");
                record.Path.ShouldEqual("path");
            }

            [Theory]
            [InlineData("C:\\debug\\")]
            [InlineData("")]
            [InlineData("::")]
            [InlineData("::C:\\debug")]
            [InlineData("C:\\debug::")]
            public void It_throws_ArgumentException_when_path_is_invalid(string path)
            {
                Assert.Throws<ArgumentException>(() => new Record(path, 0));
            }
        }
    }
}

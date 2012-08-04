using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Jump.Location.Specs
{
    public class DatabaseSpec
    {
        [Fact]
        public void It_adds_a_new_record_by_path()
        {
            var db = new Database();
            db.Add("full::path");
            db.Records.Count().ShouldEqual(1);
        }

        [Fact]
        public void It_adds_a_new_record_by_object()
        {
            var db = new Database();
            db.Add(Mock.Of<IRecord>());
            db.Records.Count().ShouldEqual(1);
        }
    }
}

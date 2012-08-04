using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Jump.Location.Specs
{
    public class DatabaseSpec
    {
        public class DescribeAdding
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

        public class DescribeFindingByFullName
        {
            [Fact]
            public void It_returns_the_matching_record()
            {
                var db = new Database();
                db.Add(Mock.Of<IRecord>(x => x.FullName == "foo"));
                db.Add(Mock.Of<IRecord>(x => x.FullName == "bar"));

                db.GetByFullName("bar").ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_a_record_when_missing()
            {
                var db = new Database();
                db.Add(Mock.Of<IRecord>(x => x.FullName == "foo"));
                db.Add(Mock.Of<IRecord>(x => x.FullName == "bar"));

                var baz = db.GetByFullName("foo::bar");
                baz.ShouldNotBeNull();
                baz.ShouldBeType<Record>();
            }

            [Fact]
            public void It_adds_the_missing_record_to_its_list()
            {
                var db = new Database();
                db.Add(Mock.Of<IRecord>(x => x.FullName == "foo"));
                db.Add(Mock.Of<IRecord>(x => x.FullName == "bar"));

                db.GetByFullName("foo::bar");
                db.Records.Count().ShouldEqual(3);
            }
        }
    }
}

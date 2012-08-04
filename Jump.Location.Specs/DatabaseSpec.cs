using System.Linq;
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
            db.AddRecord("full::path");
            db.Records.Count().ShouldEqual(1);
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Should;
using Xunit;
using Xunit.Extensions;

namespace Jump.Location.Specs
{
    public class FileStoreProviderSpec
    {
        public class DescribeRevive : IDisposable
        {
            private readonly string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            public void Dispose()
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }

            [Fact]
            public void It_can_Revive_a_single_record_of_correctly_formatted_text()
            {
                var lines = new[] {"8.5\tFS::C:\\blah"};
                File.WriteAllLines(path, lines);
                var provider = new FileStoreProvider(path);

                var db = provider.Revive();
                db.Records.Count().ShouldEqual(1);
                db.Records.First().Weight.ShouldEqual(8.5M);
            }

            [Fact]
            public void It_can_Revive_multiple_records_of_correctly_formatted_text()
            {
                var lines = new[]
                    {
                        "8.5\tFS::C:\\blah",
                        "8.5\tFS::C:\\blah",
                        "8.5\tFS::C:\\blah",
                    };
                File.WriteAllLines(path, lines);
                var provider = new FileStoreProvider(path);

                var db = provider.Revive();
                db.Records.Count().ShouldEqual(3);
            }

            [Fact]
            public void It_can_Revive_a_record_with_invalid_weight()
            {
                var lines = new[] {"INVALID_WEIGHT\tFS::C:\\blah"};
                File.WriteAllLines(path, lines);
                var provider = new FileStoreProvider(path);

                var db = provider.Revive();
                db.Records.Count().ShouldEqual(1);
                db.Records.First().Weight.ShouldEqual(0M);
                db.Records.First().Path.ShouldEqual("C:\\blah");
            }

            [Fact]
            public void It_skips_blank_and_empty_lines()
            {
                var lines = new[]
                    {
                        "",
                        "8.5\tFS::C:\\blah",
                        "    ",
                    };
                File.WriteAllLines(path, lines);
                var provider = new FileStoreProvider(path);

                var db = provider.Revive();
                db.Records.Count().ShouldEqual(1);
            }

            [Fact]
            public void It_throws_InvalidOperationException_when_row_doesnt_have_2_columns()
            {
                var lines = new[] {"8.5    FS::C:\\blah"};
                File.WriteAllLines(path, lines);
                var provider = new FileStoreProvider(path);

                Assert.Throws<InvalidOperationException>(() => provider.Revive());
            }
        }

        public class DescribeSave : IDisposable
        {
            private readonly string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            public void Dispose()
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }

            [Fact]
            public void It_can_save_a_single_record()
            {
                var db = new Database();
                db.Add(new Record("FS::C:\\data", 42M));
                var provider = new FileStoreProvider(path);
                provider.Save(db);

                var contents = File.ReadAllText(path);
                contents.ShouldEqual("42\tFS::C:\\data\r\n");
            }

            [Fact]
            public void It_can_save_a_single_record_with_weight_as_decimal_value()
            {
                var db = new Database();
                db.Add(new Record("FS::C:\\data", 42.5M));
                var provider = new FileStoreProvider(path);
                provider.Save(db);

                var contents = File.ReadAllText(path);
                contents.ShouldEqual("42.5\tFS::C:\\data\r\n");
            }
        }
    }
}

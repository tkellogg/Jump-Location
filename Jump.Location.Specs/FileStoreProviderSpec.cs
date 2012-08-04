using System;
using System.IO;
using System.Linq;
using Should;
using Xunit;

namespace Jump.Location.Specs
{
    public class FileStoreProviderSpec : IDisposable
    {
        private readonly string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        public void Dispose()
        {
            try { File.Delete(path); }
            catch { }
        }

        [Fact]
        public void It_can_Revive_a_single_record_of_correctly_formatted_text()
        {
            var lines = new[] { "FS::C:\\blah\t8.5" };
            File.WriteAllLines(path, lines);
            var provider = new FileStoreProvider(path);

            var db = provider.Revive();
            db.Records.Count().ShouldEqual(1);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using Moq;
using Should;
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

        public class DescribeFindBest
        {
            private readonly Mock<IDatabase> dbMock;
            private readonly Mock<IFileStoreProvider> fsMock;
            private readonly CommandController controller;

            public DescribeFindBest()
            {
                dbMock = new Mock<IDatabase>();
                fsMock = new Mock<IFileStoreProvider>();
                controller = new CommandController(dbMock.Object, fsMock.Object);
            }

            [Fact]
            public void Exact_match_on_last_segment_from_a_DB_of_one()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Exact_match_on_last_segment_from_a_DB_of_many()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\Kerianne", 10M), 
                        new Record(@"FS::C:\Users\lthompson", 10M), 
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void No_match_returns_null()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("notfound");
                record.ShouldBeNull();
            }

            [Fact]
            public void Exact_match_on_last_segment_is_case_insensitive()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\TKELLOGG", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\TKELLOGG");
            }

            [Fact]
            public void Last_segment_only_starts_with()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("t");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Matches_substring_of_last_segment()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("ell");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            #region Multiple arguments

            [Fact]
            public void Doesnt_match_only_middle_segments()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("users");
                record.ShouldBeNull();
            }

            [Fact]
            public void Exact_match_of_middle_segment_and_beginning_of_last()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("users", "tk");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Middle_segment_starts_with_search_term_and_beginning_of_last()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("user", "tk");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Substring_of_middle_segment_and_beginning_of_last()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("ers", "tk");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Right_match_for_two_segments()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", 10M), 
                        new Record(@"FS::C:\Users\bar", 20M),
                    });

                var record = controller.FindBest("ers", "foo");
                record.Path.ShouldEqual(@"C:\Users\foo");
            }

            [Fact]
            public void Allow_column_in_search_terms()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", 20M), 
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("c:", "tke");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Same_search_term_3_times_for_last_segment()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", 10M), 
                        new Record(@"FS::C:\Users\bar", 20M),
                    });

                var record = controller.FindBest("foo", "foo", "foo");
                record.Path.ShouldEqual(@"C:\Users\foo");
            }

            [Fact]
            public void Same_search_term_2_times_for_midle_segment()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", 10M), 
                        new Record(@"FS::C:\Users\bar", 20M),
                    });

                var record = controller.FindBest("Users", "Users", "oo");
                record.Path.ShouldEqual(@"C:\Users\foo");
            }

            [Fact]
            public void One_of_segments_is_full_path()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", 10M), 
                        new Record(@"FS::C:\Users\bar", 20M),
                    });

                var record = controller.FindBest("ers", @"C:\Users\foo");
                record.Path.ShouldEqual(@"C:\Users\foo");
            }
            
            #endregion

            [Fact]
            public void Matches_are_ordered_by_weights()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                        new Record(@"FS::C:\Users\tkellogg2", 13M), 
                        new Record(@"FS::C:\Users\tkellogg3", 15M), 
                    });

                var record = controller.GetMatchesForSearchTerm("");
                record.Select(x => x.Path).ToArray()
                    .ShouldEqual(new[]{@"C:\Users\tkellogg3", @"C:\Users\tkellogg2", @"C:\Users\tkellogg"});
            }

            [Fact]
            public void It_doesnt_include_negative_weights()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\foo", -10M), 
                    });

                var record = controller.GetMatchesForSearchTerm("foo");
                record.Any().ShouldBeFalse("Negative record should not have been matched");
            }

            [Fact]
            public void It_updates_the_database_if_filesystem_is_newer()
            {
                fsMock.Setup(x => x.LastChangedDate).Returns(DateTime.Now.AddHours(1));
                fsMock.Setup(x => x.Revive()).Returns(Mock.Of<IDatabase>());

                controller.FindBest("");

                fsMock.VerifyAll();
            }

            [Fact]
            public void Empty_query_return_most_popular()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                        new Record(@"FS::C:\Users\tkellogg2", 13M), 
                        new Record(@"FS::C:\Users\tkellogg3", 15M), 
                    });

                var record = controller.GetMatchesForSearchTerm(new string[] {});
                record.Select(x => x.Path).ToArray()
                    .ShouldEqual(new[] { @"C:\Users\tkellogg3", @"C:\Users\tkellogg2", @"C:\Users\tkellogg" });
            }
            
        }
    }
}

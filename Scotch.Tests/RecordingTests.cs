using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;

namespace Scotch.Tests
{
    public class RecordingTests : IDisposable
    {
        private readonly string _newCassettePath;
        private readonly string _testCassettePath;

        public RecordingTests()
        {
            var sourceFileDirectory = TestHelpers.GetSourceFileDirectory();
            _newCassettePath = Path.Combine(sourceFileDirectory, "newCassette.json");

            var origTestCassette = Path.Combine(sourceFileDirectory, "TestCassette.json");
            _testCassettePath = Path.Combine(sourceFileDirectory, "TestCassette_copy.json");
            File.Copy(origTestCassette, _testCassettePath);
        }

        public async Task CreatesCassetteFile()
        {
            var httpClient = Scotch.GetHttpClient(_newCassettePath, ScotchMode.Recording);

            var albumService = new AlbumService(httpClient);
            var albums = await albumService.GetAllAsync();
            albums.Count.ShouldBeGreaterThan(0);
            File.Exists(_newCassettePath).ShouldBeTrue();
        }

        public async Task AppendsNewInteractionsToCassetteFile()
        {
            var preInteractionsInCassette = Cassette.ReadCassette(_testCassettePath);
            preInteractionsInCassette.Count().ShouldBe(3);

            var httpClient = Scotch.GetHttpClient(_testCassettePath, ScotchMode.Recording);

            var albumService = new AlbumService(httpClient);
            var album1 = await albumService.GetAsync(4);
            var album2 = await albumService.GetAsync(5);

            album1.Id.ShouldBe(4);
            album2.Id.ShouldBe(5);

            var postInteractionsInCassette = Cassette.ReadCassette(_testCassettePath);
            postInteractionsInCassette.Count().ShouldBe(5);
        }

        public async Task ReplaceMatchingInteractionInCassetteFile()
        {
            var originalInteractionsInCassette = Cassette.ReadCassette(_testCassettePath).ToList();
            originalInteractionsInCassette.Count.ShouldBe(3);
            var originalRecordedTime1 = originalInteractionsInCassette.ElementAt(0).RecordedAt;
            var originalRecordedTime2 = originalInteractionsInCassette.ElementAt(1).RecordedAt;
            var originalRecordedTime3 = originalInteractionsInCassette.ElementAt(2).RecordedAt;

            var httpClient = Scotch.GetHttpClient(_testCassettePath, ScotchMode.Recording);

            var albumService = new AlbumService(httpClient);
            var album = await albumService.GetAsync(2);

            album.Id.ShouldBe(2);

            var newInteractionsInCassette = Cassette.ReadCassette(_testCassettePath).ToList();
            newInteractionsInCassette.Count.ShouldBe(3);

            var newRecordedTime1 = newInteractionsInCassette.ElementAt(0).RecordedAt;
            var newRecordedTime2 = newInteractionsInCassette.ElementAt(1).RecordedAt;
            var newRecordedTime3 = newInteractionsInCassette.ElementAt(2).RecordedAt;

            originalRecordedTime1.ShouldBe(newRecordedTime1);
            originalRecordedTime2.ShouldNotBe(newRecordedTime2);
            originalRecordedTime3.ShouldBe(newRecordedTime3);
        }

        public void Dispose()
        {
            File.Delete(_newCassettePath);
            File.Delete(_testCassettePath);
        }
    }
}
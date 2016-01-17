using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Shouldly;

namespace Scotch.Tests
{
    public class RecordingTests : IDisposable
    {
        private readonly string _cassettePath;
        private readonly string _appendNewTestCassettePath;
        private readonly string _replaceMatchingTestCassettePath;

        public RecordingTests()
        {
            var sourceFileDirectory = GetSourceFileDirectory();
            _cassettePath = Path.Combine(sourceFileDirectory, "testCassette.json");

            var sourceAppendNewTestCassette = Path.Combine(sourceFileDirectory, "TestAppendNew.json");
            _appendNewTestCassettePath = Path.Combine(sourceFileDirectory, "TestAppendNew_copy.json");
            File.Copy(sourceAppendNewTestCassette, _appendNewTestCassettePath);

            var sourceReplaceMatchingTestCassette = Path.Combine(sourceFileDirectory, "TestReplaceMatching.json");
            _replaceMatchingTestCassettePath = Path.Combine(sourceFileDirectory, "TestReplaceMatching_copy.json");
            File.Copy(sourceReplaceMatchingTestCassette, _replaceMatchingTestCassettePath);
        }

        public async Task CreatesCassetteFile()
        {
            var recordingHandler = new RecordingHandler(_cassettePath);
            var httpClient = new HttpClient(recordingHandler);

            var albumService = new AlbumService(httpClient);
            var albums = await albumService.GetAllAsync();
            albums.Count.ShouldBeGreaterThan(0);
            File.Exists(_cassettePath).ShouldBeTrue();
        }

        public async Task AppendsNewInteractionsToCassetteFile()
        {
            var originalInteractionsInCassette = Cassette.ReadCassette(_appendNewTestCassettePath);
            originalInteractionsInCassette.Count().ShouldBe(1);

            var recordingHandler = new RecordingHandler(_appendNewTestCassettePath);
            var httpClient = new HttpClient(recordingHandler);

            var albumService = new AlbumService(httpClient);
            var album1 = await albumService.GetAsync(2);
            var album2 = await albumService.GetAsync(3);

            album1.Id.ShouldBe(2);
            album2.Id.ShouldBe(3);

            var newInteractionsInCassette = Cassette.ReadCassette(_appendNewTestCassettePath);
            newInteractionsInCassette.Count().ShouldBe(3);
        }

        public async Task ReplaceMatchingInteractionInCassetteFile()
        {
            var originalInteractionsInCassette = Cassette.ReadCassette(_replaceMatchingTestCassettePath).ToList();
            originalInteractionsInCassette.Count.ShouldBe(3);
            var originalRecordedTime1 = originalInteractionsInCassette.ElementAt(0).RecordedAt;
            var originalRecordedTime2 = originalInteractionsInCassette.ElementAt(1).RecordedAt;
            var originalRecordedTime3 = originalInteractionsInCassette.ElementAt(2).RecordedAt;

            var recordingHandler = new RecordingHandler(_replaceMatchingTestCassettePath);
            var httpClient = new HttpClient(recordingHandler);

            var albumService = new AlbumService(httpClient);
            var album = await albumService.GetAsync(2);

            album.Id.ShouldBe(2);

            var newInteractionsInCassette = Cassette.ReadCassette(_replaceMatchingTestCassettePath).ToList();
            newInteractionsInCassette.Count.ShouldBe(3);

            var newRecordedTime1 = newInteractionsInCassette.ElementAt(0).RecordedAt;
            var newRecordedTime2 = newInteractionsInCassette.ElementAt(1).RecordedAt;
            var newRecordedTime3 = newInteractionsInCassette.ElementAt(2).RecordedAt;

            originalRecordedTime1.ShouldBe(newRecordedTime1);
            originalRecordedTime2.ShouldNotBe(newRecordedTime2);
            originalRecordedTime3.ShouldBe(newRecordedTime3);
        }

        private string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public void Dispose()
        {
            File.Delete(_cassettePath);
            File.Delete(_appendNewTestCassettePath);
            File.Delete(_replaceMatchingTestCassettePath);
        }
    }
}
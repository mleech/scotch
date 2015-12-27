using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Scotch;
using Xunit;

namespace SampleTestProject
{
    public class RecordingTests : IDisposable
    {
        private readonly string _cassettePath;
        private readonly string _albumByIdCassettePath;

        public RecordingTests()
        {
            var sourceFileDirectory = GetSourceFileDirectory();
            _cassettePath = Path.Combine(sourceFileDirectory, "testCassette.json");

            var sourceAlbumByIdPath = Path.Combine(sourceFileDirectory, "albumByIdCassette.json");
            _albumByIdCassettePath = Path.Combine(sourceFileDirectory, "albumByIdCassette_copy.json");
            File.Copy(sourceAlbumByIdPath, _albumByIdCassettePath);
        }

        [Fact]
        public async Task CreatesCassetteFile()
        {
            var recordingHandler = new RecordingHandler(_cassettePath);
            var httpClient = new HttpClient(recordingHandler);

            var albumService = new AlbumService(httpClient);
            var albums = await albumService.GetAllAsync();
            Assert.True(albums.Count > 0);
            Assert.True(File.Exists(_cassettePath));
        }

        [Fact]
        public async Task AppendsNewInteractionToCassetteFile()
        {
            var originalInteractionsInCassette = Cassette.ReadCassette(_albumByIdCassettePath);
            Assert.Equal(1, originalInteractionsInCassette.Length);

            var recordingHandler = new RecordingHandler(_albumByIdCassettePath);
            var httpClient = new HttpClient(recordingHandler);

            var albumService = new AlbumService(httpClient);
            var album = await albumService.GetAsync(2);
            Assert.Equal(2, album.Id);

            var newInteractionsInCassette = Cassette.ReadCassette(_albumByIdCassettePath);
            Assert.Equal(2, newInteractionsInCassette.Length);
        }

        private string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public void Dispose()
        {
            File.Delete(_cassettePath);
            File.Delete(_albumByIdCassettePath);
        }
    }
}
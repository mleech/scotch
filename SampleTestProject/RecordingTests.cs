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

        public RecordingTests()
        {
            var sourceFileDirectory = GetSourceFileDirectory();
            _cassettePath = Path.Combine(sourceFileDirectory, "testCassette.txt");
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

        private string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public void Dispose()
        {
            File.Delete(_cassettePath);
        }
    }
}
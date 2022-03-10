using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scotch.Tests
{
    [TestClass]
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

        [TestMethod]
        public async Task CreatesCassetteFile()
        {
            var httpClient = HttpClients.NewHttpClient(_newCassettePath, ScotchMode.Recording);

            var albumService = new AlbumService(httpClient);
            var albums = await albumService.GetAllAsync();
            Assert.IsTrue(albums.Count > 0);
            Assert.IsTrue(File.Exists(_newCassettePath));
        }

        [TestMethod]
        public async Task AppendsNewInteractionsToCassetteFile()
        {
            var preInteractionsInCassette = Cassette.ReadCassette(_testCassettePath);
            var currentCount = preInteractionsInCassette != null ? preInteractionsInCassette.Count() : 0;

            var httpClient = HttpClients.NewHttpClient(_testCassettePath, ScotchMode.Recording);

            var albumService = new AlbumService(httpClient);
            var album1 = await albumService.GetAsync(4);
            var album2 = await albumService.GetAsync(5);
            
            Assert.AreEqual(4, album1.Id);
            Assert.AreEqual(5, album2.Id);

            var postInteractionsInCassette = Cassette.ReadCassette(_testCassettePath);
            Assert.AreEqual(currentCount + 2, postInteractionsInCassette.Count());
        }
        
        public void Dispose()
        {
            File.Delete(_newCassettePath);
            File.Delete(_testCassettePath);
        }
    }
}
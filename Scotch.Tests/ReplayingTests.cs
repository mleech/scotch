using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scotch.Tests
{
    [TestClass]
    public class ReplayingTests
    {
        private readonly string _testCassettePath;

        public ReplayingTests()
        {
            var sourceFileDirectory = TestHelpers.GetSourceFileDirectory();
            _testCassettePath = Path.Combine(sourceFileDirectory, "TestCassette.json");
        }

        [TestMethod]
        public async Task ReplaysMatchingHttpInteractionFromCassette()
        {
            var httpClient = HttpClients.NewHttpClient(_testCassettePath, ScotchMode.Replaying);

            var albumService = new AlbumService(httpClient);
            var album = await albumService.GetAsync(2);
            
            Assert.AreEqual("Hunky Dory", album.Title);
        }

        [TestMethod]
        public async Task ReplayedResponseHasCorrectContentType()
        {
            var httpClient = HttpClients.NewHttpClient(_testCassettePath, ScotchMode.Replaying);

            var url = "https://jsonplaceholder.typicode.com/albums/2";
            var response = await httpClient.GetAsync(url);
            
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType);
        }
    }
}

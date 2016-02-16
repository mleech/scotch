using System.IO;
using System.Threading.Tasks;
using Shouldly;

namespace Scotch.Tests
{
    public class ReplayingTests
    {
        private readonly string _testCassettePath;

        public ReplayingTests()
        {
            var sourceFileDirectory = TestHelpers.GetSourceFileDirectory();
            _testCassettePath = Path.Combine(sourceFileDirectory, "TestCassette.json");
        }

        public async Task ReplaysMatchingHttpInteractionFromCassette()
        {
            var httpClient = HttpClients.NewHttpClient(_testCassettePath, ScotchMode.Replaying);

            var albumService = new AlbumService(httpClient);
            var album = await albumService.GetAsync(2);

            album.Title.ShouldBe("Hunky Dory");
        }
    }
}

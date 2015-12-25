using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SampleTestProject;
using Scotch;
using Xunit;

public class AlbumServiceTests
{
    [Fact]
    public async Task ReturnsAllAlbums()
    {
        var sourceFileDirectory = GetSourceFileDirectory();
        var cassettePath = Path.Combine(sourceFileDirectory, "testCassette.txt");
        var recordingHandler = new RecordingHandler(cassettePath);
        var httpClient = new HttpClient(recordingHandler);

        var albumService = new AlbumService(httpClient);
        var albums = await albumService.GetAllAsync();
        httpClient.Dispose();
        Assert.Equal(100, albums.Count);

    }

    private string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetDirectoryName(sourceFilePath);
    }
}
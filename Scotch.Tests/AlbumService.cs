using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Scotch.Tests
{

    public class Album
    {
        public int Id { get; }
        public string Title { get; }

        public Album(int id, string title)
        {
            Id = id;
            Title = title;
        }
    }

    public class AlbumService
    {
        private readonly HttpClient _httpClient;

        public AlbumService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<Album>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/albums");
            var jsonString = await response.Content.ReadAsStringAsync();

            var albums = JsonConvert.DeserializeObject<IList<Album>>(jsonString);

            return albums;
        }

        public async Task<Album> GetAsync(int id)
        {
            var url = $"https://jsonplaceholder.typicode.com/albums/{id}";
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();

            var album = JsonConvert.DeserializeObject<Album>(jsonString);

            return album;
        }
    }
}

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AspireWithAtlas.Web.Models;
using MongoDB.Driver;

namespace AspireWithAtlas.Web.Services
{
    public class GooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly IMongoDatabase _drinksDatabase;
        private readonly string _apiKey = "<Your Google API Key>"; // Replace with your actual API key

        public GooglePlacesService(IHttpClientFactory httpClientFactory, IMongoClient mongoClient)
        {
            _httpClient = httpClientFactory.CreateClient();
            _drinksDatabase = mongoClient.GetDatabase("drinks");

        }

        public async Task<Place> SearchPlacesAsync(string searchTerm, string location, int radius = 3000)
        {
            // Construct the API URL
            var apiUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(searchTerm)}&location={location}&radius={radius}&key={_apiKey}";

            try
            {
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                // Read the response content
                var content = await response.Content.ReadAsStringAsync();

                // Optionally, deserialize the content if you need to work with the response data
                // var result = JsonConvert.DeserializeObject<YourDesiredObjectType>(content);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var jsonDocument = JsonDocument.Parse(content);
                var places = JsonSerializer.Deserialize<Place>(content, options);

                // Save the results as a favourites list
                var favouritesCollection = _drinksDatabase.GetCollection<Result>("favourites");

                var results = places.results;

                for(int i = 0; i < 5; i++)
                {
                    var result = results[i];
                    await favouritesCollection.InsertOneAsync(result);
                }

                return places;
            }
            catch (HttpRequestException e)
            {
                // Handle any errors here
                throw new Exception("Error calling the Google Places API", e);
            }
        }
    }
}

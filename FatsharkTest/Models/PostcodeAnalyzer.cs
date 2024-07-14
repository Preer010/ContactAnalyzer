using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace FatsharkTest.Models;

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class PostcodeAnalyzer
{
    private readonly HttpClient _client;

    public PostcodeAnalyzer()
    {
        var clientHandler = new HttpClientHandler { UseProxy = false };

        _client = new HttpClient(clientHandler);
    }

    public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string postcode)
    {
        //var url = $"https://api.postcodes.io/postcodes/{postcode}";
        var url = $"https://api.postcodes.io/postcodes";
        var content = new StringContent("{\"postcodes\":[\"OX49 5NU\",\"M32 0JG\",\"NE30 1DP\"]}",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


        // var data = JObject();
        // if (data["status"].Value<int>() == 200)
        // {
        //     var result = data["result"];
        //     return (result["latitude"].Value<double>(), result["longitude"].Value<Double>());
        // }
        // else
        // {
        //     throw new Exception($"Failed to retrieve coordinates for postcode: {postcode}");
        // }
        return (2, 2);
    }

    private List<List<string>> GetPayloads(List<string> postcodes, int apiLimit)
    {
        List<List<string>> postcodeJobs = new List<List<string>>();
        if (postcodes.Count > apiLimit)
        {
            for (var i = 0; i < postcodes.Count; i += apiLimit)
            {
                List<string> postcodeChunk = postcodes.GetRange(i, Math.Min(apiLimit, postcodes.Count - i));
                postcodeJobs.Add(postcodeChunk);
            }
        }
        else
        {
            postcodeJobs.Add(postcodes);
        }

        return postcodeJobs;
    }

    public async Task<List<(double Latitude, double Longitude)>> GetBulkCoordinatesAsync(List<string> postcodes)
    {
        const int apiLimit = 100;
        const string url = $"https://api.postcodes.io/postcodes/";

        List<List<string>> postcodeJobs = GetPayloads(postcodes, 100);
        var results = new List<(double Latitude, double Longitude)>();

        foreach (var job in postcodeJobs)
        {
            var payload = new { postcodes = job };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // TODO FIGURE OUT WHY THIS SHIT WONT APPEAREREAREAREREAR
            var response = await _client.PostAsync(url, content).ConfigureAwait(false);
            
            response.EnsureSuccessStatusCode();
            var respondingStirng = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JObject.Parse(respondingStirng);


            if (data["status"].Value<int>() == 200)
            {

                
                foreach (var result in data["result"])
                {
                    var r = result["result"];
                    if (!r.HasValues)
                    {
                        continue;
                    }

                    var latitude = r["latitude"].Value<double>();
                    var longitude = r["longitude"].Value<double>();
                    results.Add((latitude, longitude));
                }
            }
            else
            {
                throw new Exception($"Failed to retrieve coordinates for bulk job");
            }
        }


        return results;
    }
}
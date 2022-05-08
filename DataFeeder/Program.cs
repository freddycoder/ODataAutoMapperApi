using System.Text;
using System.Text.Json;

using var httpClient = new HttpClient();

var headers = httpClient.DefaultRequestHeaders;

await Parallel.ForEachAsync(DataFeeder.Feeder.GenerateRandoms(10_000_000), new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (forecast, token) =>
{
    using var body = new StringContent(JsonSerializer.Serialize(forecast), Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync("http://localhost:5049/WeatherForecast", body, token);

    response.EnsureSuccessStatusCode();

    if (DateTime.Now.Millisecond == 0)
    {
        Console.Write(response.StatusCode);
        Console.Write(' ');
        Console.WriteLine(await response.Content.ReadAsStringAsync(token));
    }
});
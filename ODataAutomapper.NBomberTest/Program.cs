using DataFeeder;
using NBomber.Contracts;
using NBomber.CSharp;
using ODataAutomapperApi.Models;
using System.Collections.Concurrent;
using System.Text.Json;

const string baseUrl = "http://localhost:5049/WeatherForecast?&orderby=date&";
const int pageSize = 50;
TimeSpan interval = TimeSpan.FromDays(900);
TimeSpan stepTimeout = TimeSpan.FromSeconds(25);

using var httpClient = new HttpClient();

var step1 = Step.Create("key_based_pagination", async context =>
{
    var summary = Feeder.GetRandomSummary();
    var start = Feeder.GetRandomDate();
    var end = start + interval;

    var response = await httpClient.GetAsync(
$"{baseUrl}$takeOneElementEach={pageSize}&summary={summary}&$select=id&start={start}&end={end}");

    var jsonStr = await response.Content.ReadAsStringAsync();

    var ids = JsonSerializer.Deserialize<List<IdObj>>(jsonStr);

    if (ids == null)
        throw new InvalidOperationException("The response was empty or deserialization failed");

    var concurentSized = new ConcurrentQueue<int>();

    concurentSized.Enqueue((int)(response.Content.Headers.ContentLength ?? 0));

    await Parallel.ForEachAsync(ids, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (idObj, token) =>
    {
        var result = await httpClient.GetAsync(
$"{baseUrl}summary={summary}&$top={pageSize}&$filter=id ge {idObj.Id}&start={start}&end={end}", token);

        result.EnsureSuccessStatusCode();

        var resultJson = await result.Content.ReadAsStringAsync();

        concurentSized.Enqueue((int)(result.Content.Headers.ContentLength ?? 0));
    });

    return response.IsSuccessStatusCode
        ? Response.Ok(payload: null, (int)response.StatusCode, sizeBytes: concurentSized.Sum())
        : Response.Fail();
}, timeout: stepTimeout);

var step2 = Step.Create("skipNtake_based_pagination", async context =>
{
    var summary = Feeder.GetRandomSummary();
    var start = Feeder.GetRandomDate();
    var end = start + interval;

    var response = await httpClient.GetAsync(
$"{baseUrl}summary={summary}&start={start}&end={end}&addPaginationHeader=true");

    var concurentSized = new ConcurrentQueue<int>();

    concurentSized.Enqueue((int)(response.Content.Headers.ContentLength ?? 0));

    if (response.Headers.TryGetValues("X-Pagination", out var pagination))
    {
        var xpagination = JsonSerializer.Deserialize<PaginationModel>(pagination.Single());

        if (xpagination == null)
            throw new InvalidOperationException("Problem when deserializing X-Pagination header");

        var nbPageLeft = (xpagination.Total / pageSize) - 1;

        if (nbPageLeft > 0)
        {
            await Parallel.ForEachAsync(Enumerable.Range(2, nbPageLeft), new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (page, token) =>
            {
                var result = await httpClient.GetAsync(
    $"{baseUrl}summary={summary}&$skip={pageSize * page - 1}&$top={pageSize}&start={start}&end={end}&addPaginationHeader=true");

                result.EnsureSuccessStatusCode();

                var resultJson = await result.Content.ReadAsStringAsync();

                concurentSized.Enqueue((int)(result.Content.Headers.ContentLength ?? 0));
            });
        }
    }
    else
    {
        throw new InvalidOperationException("The response dosen't contain X-Pagination header");
    }

    return response.IsSuccessStatusCode
        ? Response.Ok(payload: null, (int)response.StatusCode, sizeBytes: concurentSized.Sum())
        : Response.Fail();
}, timeout: stepTimeout);

var scenario = ScenarioBuilder.CreateScenario("compare_keybased_vs_offset", step1, step2)
                              .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                              .WithLoadSimulations(
                                  Simulation.InjectPerSec(rate: 1, during: TimeSpan.FromSeconds(30)),
                                  Simulation.InjectPerSec(rate: 4, during: TimeSpan.FromSeconds(30)),
                                  Simulation.InjectPerSec(rate: 12, during: TimeSpan.FromSeconds(30)),
                                  Simulation.InjectPerSec(rate: 24, during: TimeSpan.FromSeconds(30))
                              );

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
using ODataAutomapperApi.Models;

namespace DataFeeder;

public class Feeder
{
    public static Random Random = new Random();

    public static string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static string GetRandomSummary()
    {
        return Summaries[Random.Next(Summaries.Length)];
    }

    public static DateTime GetRandomDate()
    {
        return DateTime.Now - TimeSpan.FromDays(Random.Next(365 * 2021));
    }

    public static IEnumerable<WeatherForecast> GenerateRandoms(int n)
    {
        int returned = 0;

        var restart = (n / 365) + 1;

        for (int q = 0; q < restart; q++)
        {
            for (int i = -(365 * 2021); i < 0; i++)
            {
                var summary = GetRandomSummary();

                yield return new WeatherForecast
                {
                    Date = DateTime.Now + TimeSpan.FromDays(i),
                    Summary = summary,
                    TemperatureC = Random.Next(-30, 30)
                };

                if (++returned >= n)
                {
                    break;
                }
            }

            if (returned >= n)
            {
                break;
            }
        }
    }
}

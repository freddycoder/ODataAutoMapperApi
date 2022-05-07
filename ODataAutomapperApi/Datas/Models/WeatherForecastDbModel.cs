using Microsoft.EntityFrameworkCore;

namespace ODataAutomapperApi.Datas.Models;

[Index(nameof(Date), nameof(SummaryNormalized), Name = "Date_SummaryNames")]
public class WeatherForecastDbModel
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    public string? SummaryNormalized { get; set; }
}

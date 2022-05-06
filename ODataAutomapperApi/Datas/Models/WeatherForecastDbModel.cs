namespace ODataAutomapperApi.Datas.Models;

public class WeatherForecastDbModel
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    public string? SummaryNormalized { get; set; }
}

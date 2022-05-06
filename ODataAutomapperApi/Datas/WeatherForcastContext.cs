using Microsoft.EntityFrameworkCore;
using ODataAutomapperApi.Datas.Models;

namespace ODataAutomapperApi.Datas;

#nullable disable

public class WeatherForcastContext : DbContext
{
    public WeatherForcastContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<WeatherForecastDbModel> WeatherForecasts { get; set; }
}

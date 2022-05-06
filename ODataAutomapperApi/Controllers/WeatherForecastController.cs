using Microsoft.AspNetCore.Mvc;
using ODataAutomapperApi.Attributes;
using ODataAutomapperApi.Datas;
using ODataAutomapperApi.Datas.Models;
using ODataAutomapperApi.Models;

namespace ODataAutomapperApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly WeatherForcastContext _context;
    private const int _defaultLimit = 50;

    public WeatherForecastController(WeatherForcastContext context)
    {
        _context = context;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    [AutoMapperQueryEnabled(source: typeof(WeatherForecastDbModel), destination: typeof(WeatherForecast[]))]
    [ProducesResponseType(200, Type = typeof(WeatherForecast[]))]
    public IQueryable Get()
    {
        var query = _context.WeatherForecasts;

        if (HttpContext.Request.Query.ContainsKey("$top") || 
            (HttpContext.Request.Query.TryGetValue("$select", out var select) && string.Equals(select, "id", StringComparison.OrdinalIgnoreCase)))
        {
            return query;
        }

        return query.Take(_defaultLimit);
    }
}

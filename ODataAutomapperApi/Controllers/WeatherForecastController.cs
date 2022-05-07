using AutoMapper;
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

    [HttpPost(Name = "PostWeatherForecast")]
    [ProducesResponseType(201, Type = typeof(WeatherForecast))]
    public async Task<IActionResult> Post([FromBody] WeatherForecast weatherForecast, CancellationToken token)
    {
        var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();

        var forecast = mapper.Map<WeatherForecastDbModel>(weatherForecast);

        await _context.AddAsync(forecast, token);

        await _context.SaveChangesAsync(token);

        return Created("/WeaterForecast/" + forecast.Id, mapper.Map<WeatherForecast>(forecast));
    }
}

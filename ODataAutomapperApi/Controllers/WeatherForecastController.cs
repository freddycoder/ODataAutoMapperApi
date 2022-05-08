using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataAutomapperApi.Attributes;
using ODataAutomapperApi.Datas;
using ODataAutomapperApi.Datas.Models;
using ODataAutomapperApi.Models;
using System.Text.Json;

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
    public async Task<IQueryable> Get([FromQuery] string? summary, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] bool addPaginationHeader)
    {
        IQueryable<WeatherForecastDbModel> query = _context.WeatherForecasts;

        if (string.IsNullOrWhiteSpace(summary) == false)
        {
            query = query.Where(q => q.Summary == summary);
        }

        if (start.HasValue)
        {
            query = query.Where(d => d.Date >= start);
        }

        if (end.HasValue)
        {
            query = query.Where(d => d.Date <= end);
        }

        if (addPaginationHeader)
        {
            var countQuery = query;

            if (HttpContext.Request.Query.TryGetValue("$filter", out var filter))
            {
                var filterValue = filter.ToString().Split(' ').Last().Replace("\'", "");

                countQuery = countQuery.Where(d => d.Summary == filterValue);
            }

            var total = await countQuery.CountAsync();

            var paginationModel = new PaginationModel
            {
                Total = total
            };

            HttpContext.Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationModel));
        }

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

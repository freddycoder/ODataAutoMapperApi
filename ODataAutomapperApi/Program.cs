using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using ODataAutomapperApi.Attributes.Contexts;
using ODataAutomapperApi.Datas;
using ODataAutomapperApi.Datas.Models;
using ODataAutomapperApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TakeOneElementEachContext>();
builder.Services.AddControllers().AddOData(options =>
{
    options.EnableQueryFeatures();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(config =>
{
    config.CreateMap<WeatherForecast, WeatherForecastDbModel>().ReverseMap();
});

builder.Services.AddDbContext<WeatherForcastContext>(options =>
{
    if (builder.Configuration["ConnectionStrings:SQLServer"] != null)
    {
        options.UseSqlServer(builder.Configuration["ConnectionStrings:SQLServer"], sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        });
    }
    else
    {
        options.UseInMemoryDatabase(nameof(WeatherForcastContext));
    }
});

var app = builder.Build();

if (builder.Configuration["ConnectionStrings:SQLServer"] == null)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<WeatherForcastContext>();

        var random = new Random();
        string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        for (int i = -(365 * 400); i < 0; i++)
        {
            var summary = summaries[random.Next(summaries.Length)];

            context.Add<WeatherForecastDbModel>(new WeatherForecastDbModel
            {
                Date = DateTime.Now + TimeSpan.FromDays(i),
                Summary = summary,
                SummaryNormalized = summary.ToUpper().Trim(),
                TemperatureC = random.Next(-30, 30)
            });
        }

        context.SaveChanges();
    }
}
else
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<WeatherForcastContext>();

        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

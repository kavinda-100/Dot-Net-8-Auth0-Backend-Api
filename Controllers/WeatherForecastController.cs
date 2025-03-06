using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0_dot_net_8_api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
    
    [HttpGet("all")]
    [Authorize]
    public IActionResult Private()
    {
        return Ok(new
        {
            Message = "Hello from a private endpoint!"
        });
    }
    
    [HttpGet("admin-only")]
    [Authorize]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            Message = "Admin only endpoint! and not edit privileges"
        });
    }

    [HttpGet("admin-private-scoped")]
    // [Authorize("admin:edit")]
    [Authorize]
    public IActionResult Scoped()
    {
        return Ok(new
        {
            Message = "Admin only endpoint! and with edit privileges"
        });
    }
}
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("emissions")]
public class CarbonAwareController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;
    private readonly ICarbonAware _plugin;

    public CarbonAwareController(ILogger<CarbonAwareController> logger, ICarbonAware plugin)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocations/best")]
    public async Task<IActionResult> GetBestEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.LOCATIONS, locations.ToList() },
            { CarbonAwareConstants.START, time ?? DateTime.Now },
            { CarbonAwareConstants.DURATION, durationMinutes },
            { CarbonAwareConstants.LOWEST, true }
        };
        if(toTime != null) 
        {
            props[CarbonAwareConstants.END] = toTime;
        }
        return await GetEmissionsDataAsync(props);
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocations")]
    public async Task<IActionResult> GetEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.LOCATIONS, locations.ToList() },
            { CarbonAwareConstants.START, time ?? DateTime.Now },
            { CarbonAwareConstants.DURATION, durationMinutes },
        };
        
        if(toTime != null) 
        {
            props[CarbonAwareConstants.END] = toTime;
        }
        return await GetEmissionsDataAsync(props);
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetEmissionsDataForLocationByTime(string location, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.LOCATIONS, new List<string>(){ location } },
            { CarbonAwareConstants.START, time ?? DateTime.Now },
            { CarbonAwareConstants.DURATION, durationMinutes },
        };
        
        if(toTime != null) 
        {
            props[CarbonAwareConstants.END] = toTime;
        }
        return await GetEmissionsDataAsync(props);
    }

    /// <summary>
    /// Given a dictionary of properties, handles call to GetEmissionsDataAsync including logging and response handling.
    /// </summary>
    /// <param name="props"> Dictionary of properties to call plugin. </param>
    /// <returns>Result of the plugin call or resulting status response</returns>
    private async Task<IActionResult> GetEmissionsDataAsync(Dictionary<string, object> props)
    {
        // NOTE: Any auth information would need to be redacted from logging
        _logger.LogInformation("Calling plugin GetEmissionsDataAsync with paylod {@props}", props);
        try
        {
            var response = await _plugin.GetEmissionsDataAsync(props);
            if (response.Any())
            {
                _logger.LogInformation("Plugin call successful with result {@result}", response);
                return Ok(response);
            }
            else
            {
                _logger.LogInformation("Plugin call returned empty result");
                return StatusCode(204, response);
            }
        } 
        catch (Exception ex)
        {
            _logger.LogError("Exception occured during plugin execution", ex);
            return StatusCode(400, ex);
        }
    }
}

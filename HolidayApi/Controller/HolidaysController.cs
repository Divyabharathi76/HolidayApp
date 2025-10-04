using HolidayApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HolidayApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly HolidayService _holidayService;
        private readonly ILogger<HolidaysController> _logger;

        public HolidaysController(HolidayService holidayService, ILogger<HolidaysController> logger)
        {
            _holidayService = holidayService;
            _logger = logger;
        }

        /// Fetch holidays from  API then save to database.
        [HttpPost("fetch/{countryCode}/{year}")]
        public async Task<IActionResult> FetchAndSaveHolidays(string countryCode, int year)
        {
            try
            {
                var holidays = await _holidayService.FetchHolidaysFromApiAsync(countryCode, year);
                await _holidayService.SaveHolidaysToDbAsync(holidays);

                return Ok(new
                {
                    Message = $"Fetched and saved {holidays.Count} holidays for {countryCode} in {year}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FetchAndSaveHolidays for {Country}-{Year}", countryCode, year);
                return BadRequest(ex);
            }
        }

        /// Return last 3 holidays
        [HttpGet("last3/{name}")]
        public async Task<IActionResult> GetLast3Holidays(string name)
        {
            try
            {
                var last3 = await _holidayService.GetLast3HolidaysAsync(name);
                return Ok(last3); 
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching last 3 holidays for {Name}", name);
                return BadRequest(ex);
            }
        }

        /// Get weekdays Holidays that too with type public
        [HttpGet("nonweekendcount/{year}")]
        public async Task<IActionResult> GetNonWeekendHolidayCount(int year, [FromQuery] string countries)
        {
            try
            {
                if (!TryParseCountries(countries, null, out var countryList))
                    return BadRequest("Please provide at least one country code.");

                var result = await _holidayService.GetNonWeekendHolidayCountForCountriesAsync(year, countryList);
                return Ok(result);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Error fetching non-weekend holiday count for year {Year}", year);
                return BadRequest(ex);
            }
        }

        /// Get Common Holidays
        [HttpGet("common/{year}")]
        public async Task<IActionResult> GetCommonHolidays(int year, [FromQuery] string countries)
        {
            if (!TryParseCountries(countries, 2, out var countryList))
                return BadRequest("Please provide exactly 2 country codes.");
            try
            {
                var common = await _holidayService.GetCommonHolidaysAsync(countryList[0], countryList[1], year);
                return Ok(common);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching common holidays for countries {Country1} and {Country2} in year {Year}", countryList[0], countryList[1], year);
                return BadRequest(ex);
            }
        }

        private bool TryParseCountries(string countries, int? expectedCount, out List<string> parsedCountries)
        {
            parsedCountries = countries?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList() ?? new List<string>();

            if (expectedCount.HasValue && parsedCountries.Count != expectedCount.Value)
                return false;

            return parsedCountries.Any();
        }
    }
}

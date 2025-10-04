using HolidayApi.Data;
using HolidayApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HolidayApi.Services
{
    public class HolidayService
    {
        private readonly HolidaysDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HolidayService> _logger;

        public HolidayService(HolidaysDbContext dbContext, HttpClient httpClient, ILogger<HolidayService> logger)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<NagerHolidayDto>> FetchHolidaysFromApiAsync(string countryCode, int year)
        {
            var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<NagerHolidayDto>>(url);
                return response ?? new List<NagerHolidayDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching holidays for {Country}-{Year}", countryCode, year);
                return new List<NagerHolidayDto>();
            }
        }

        public async Task SaveHolidaysToDbAsync(List<NagerHolidayDto> holidays)
        {
            try
            {
                foreach (var h in holidays)
                {
                    var existingHoliday = await _dbContext.Holidays
                        .FirstOrDefaultAsync(x => x.CountryCode == h.CountryCode && x.Date == h.Date);

                    if (existingHoliday == null)
                    {
                        Holidays _data = new Holidays();
                        _data.CountryCode = h.CountryCode;
                        _data.LocalName = h.LocalName;
                        _data.Name = h.Name;
                        _data.Date = h.Date;
                        _data.Types = h.Types != null ? string.Join(",", h.Types) : null;
                        _dbContext.Holidays.Add(_data);
                    }
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Holidays saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving holidays to the database");
            }
        }

        public async Task<List<HolidayDto>> GetLast3HolidaysAsync(string CountryCode)
        {
            try
            {
                return await _dbContext.Holidays
                    .Where(h => h.CountryCode == CountryCode)
                    .OrderByDescending(h => h.Date)
                    .Take(3)
                    .Select(h => new HolidayDto
                    {
                        Date = h.Date,
                        Name = h.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching last 3 holidays for {Name}", CountryCode);
                return new List<HolidayDto>();
            }
        }

        public async Task<List<NonWeekendHolidayCountDto>> GetNonWeekendHolidayCountForCountriesAsync(int year, List<string> countryCodes)
        {
            try
            {
                var data = await _dbContext.Holidays
                    .Where(h => countryCodes.Contains(h.CountryCode) && h.Date.Year == year)
                    .ToListAsync(); 

                var query = data
                    .Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday &&
                                h.Date.DayOfWeek != DayOfWeek.Sunday &&
                                h.Types != null && h.Types.Contains("Public"))
                    .GroupBy(h => h.CountryCode)
                    .Select(g => new NonWeekendHolidayCountDto
                    {
                        Country = g.Key,
                        NonWeekendHolidays = g.Count()
                    })
                    .OrderByDescending(g => g.NonWeekendHolidays)
                    .ToList();

                return query;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting non-weekend holidays for year {Year}", year);
                return new List<NonWeekendHolidayCountDto>();
            }
        }


        public async Task<List<CommonHolidayDto>> GetCommonHolidaysAsync(string country1, string country2, int year)
        {
            try
            {
                var holidays1 = await _dbContext.Holidays.Where(h => h.CountryCode == country1 && h.Date.Year == year).ToListAsync();
                var holidays2 = await _dbContext.Holidays.Where(h => h.CountryCode == country2 && h.Date.Year == year).ToListAsync();

                var common = (from h1 in holidays1
                    from h2 in holidays2
                    where h1.Date.Date == h2.Date.Date
                    select new CommonHolidayDto
                    {
                        Date = h1.Date.Date,
                        Country1Name = h1.LocalName,
                        Country2Name = h2.LocalName
                    })
                    .GroupBy(x => x.Date)
                    .Select(g => g.First())
                    .ToList();

                return common;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching common holidays for {Country1} and {Country2} in year {Year}", country1, country2, year);
                return new List<CommonHolidayDto>();
            }
        }
    }
}

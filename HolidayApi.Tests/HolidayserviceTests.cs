using HolidayApi.Data;
using HolidayApi.Models;
using HolidayApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HolidayApi.Tests
{
    [TestFixture]
    public class HolidayServiceTests
    {
        private HolidaysDbContext _context;
        private HolidayService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<HolidaysDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HolidaysDbContext(options);

            var httpClient = new HttpClient(); 
            var loggerMock = new Mock<ILogger<HolidayService>>();

            _service = new HolidayService(_context, httpClient, loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetLast3HolidaysAsync_ReturnsCorrectCount()
        {
            // Arrange
            _context.Holidays.AddRange(
                new Holidays { CountryCode = "US", Name = "TestHoliday", LocalName = "TestLocal1", Date = new DateTime(2023, 12, 25), Types = "Public" },
                new Holidays { CountryCode = "US", Name = "TestHoliday", LocalName = "TestLocal2", Date = new DateTime(2022, 12, 25), Types = "Public" },
                new Holidays { CountryCode = "US", Name = "TestHoliday", LocalName = "TestLocal3", Date = new DateTime(2021, 12, 25), Types = "Public" },
                new Holidays { CountryCode = "US", Name = "TestHoliday", LocalName = "TestLocal4", Date = new DateTime(2020, 12, 25), Types = "Public" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLast3HolidaysAsync("US");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Date, Is.EqualTo(new DateTime(2023, 12, 25))); // Latest date first
        }

        [Test]
        public async Task GetNonWeekendHolidayCountForCountriesAsync_ReturnsCorrectCount()
        {
            // Arrange
            _context.Holidays.AddRange(
                new Holidays { CountryCode = "US", Name = "Holiday1", LocalName = "Local1", Date = new DateTime(2024, 1, 1), Types = "Public" }, // Monday
                new Holidays { CountryCode = "US", Name = "Holiday2", LocalName = "Local2", Date = new DateTime(2024, 1, 6), Types = "Public" }, // Saturday
                new Holidays { CountryCode = "IN", Name = "Holiday3", LocalName = "Local3", Date = new DateTime(2024, 1, 2), Types = "Public" }  // Tuesday
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetNonWeekendHolidayCountForCountriesAsync(2024, new List<string> { "US", "IN" });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Find(x => x.Country == "US")!.NonWeekendHolidays, Is.EqualTo(1));
            Assert.That(result.Find(x => x.Country == "IN")!.NonWeekendHolidays, Is.EqualTo(1));
        }
        [Test]
        public async Task GetNonWeekendHolidayCountForOneCountryAsync_ReturnsCorrectCount()
        {
            // Arrange
            _context.Holidays.AddRange(
                new Holidays { CountryCode = "US", Name = "Holiday1", LocalName = "Local1", Date = new DateTime(2024, 1, 1), Types = "Public" }, // Monday
                new Holidays { CountryCode = "US", Name = "Holiday2", LocalName = "Local2", Date = new DateTime(2024, 1, 6), Types = "Public" }, // Saturday
                new Holidays { CountryCode = "IN", Name = "Holiday3", LocalName = "Local3", Date = new DateTime(2024, 1, 2), Types = "Public" }  // Tuesday
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetNonWeekendHolidayCountForCountriesAsync(2024, new List<string> { "US" });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Find(x => x.Country == "US")!.NonWeekendHolidays, Is.EqualTo(1));
        }

        [Test]
        public async Task GetCommonHolidaysAsync_ReturnsOnlyCommonDates()
        {
            // Arrange
            _context.Holidays.AddRange(
                new Holidays { CountryCode = "US", Name = "Holiday1", LocalName = "Local1", Date = new DateTime(2024, 1, 1), Types = "Public" },
                new Holidays { CountryCode = "US", Name = "Holiday2", LocalName = "Local2", Date = new DateTime(2024, 2, 1), Types = "Public" },
                new Holidays { CountryCode = "IN", Name = "HolidayA", LocalName = "LocalA", Date = new DateTime(2024, 1, 1), Types = "Public" },
                new Holidays { CountryCode = "IN", Name = "HolidayB", LocalName = "LocalB", Date = new DateTime(2024, 3, 1), Types = "Public" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCommonHolidaysAsync("US", "IN", 2024);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Date, Is.EqualTo(new DateTime(2024, 1, 1)));
            Assert.That(result[0].Country1Name, Is.EqualTo("Local1"));
            Assert.That(result[0].Country2Name, Is.EqualTo("LocalA"));
        }
        //         [Test]
        // public void GetLast3HolidaysAsync_ThrowsArgumentException_ForEmptyName()
        // {
        //     // Arrange
        //     string invalidName = "";

        //     // Act & Assert
        //     var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        //         await _service.GetLast3HolidaysAsync(invalidName)
        //     );

        //     Assert.That(ex!.Message, Is.EqualTo("Name cannot be null or empty."));
        // }
    }
}

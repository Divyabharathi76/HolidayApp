using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolidayApi.Models
{
    public class Holidays
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        public string LocalName { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [NotMapped] // Don't store in DB, compute on the fly
        public int Year => Date.Year;
        [Required]
        public string Types { get; set; } = string.Empty;
        // public string? Types { get; set; }
    }
    // DTO for non-weekend holiday count
    public class NonWeekendHolidayCountDto
    {
        public string Country { get; set; } = null!;
        public int NonWeekendHolidays { get; set; }
    }
    public class HolidayDto
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = null!;
    }
    public class CommonHolidayDto
    {
        public DateTime Date { get; set; }
        public string Country1Name { get; set; } = string.Empty;
        public string Country2Name { get; set; } = string.Empty;
    }
}

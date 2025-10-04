using System;
using System.Text.Json.Serialization;

namespace HolidayApi.Models
{
    /// Deserialize JSON
    public class NagerHolidayDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("localName")]
        public string LocalName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public string[] Types { get; set; } = [];
    }
}

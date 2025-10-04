using Microsoft.EntityFrameworkCore;
using HolidayApi.Models;

namespace HolidayApi.Data
{
    public class HolidaysDbContext : DbContext
    {
        public DbSet<Holidays> Holidays { get; set; } = null!;

        public HolidaysDbContext(DbContextOptions<HolidaysDbContext> options)
            : base(options) { }

       
    }
}

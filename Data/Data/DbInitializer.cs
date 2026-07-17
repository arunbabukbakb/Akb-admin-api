using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Migration Failed: {ex.Message}");
                throw;
            }
        }
    }
}

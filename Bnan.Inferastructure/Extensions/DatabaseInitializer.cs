using Microsoft.EntityFrameworkCore;

namespace Bnan.Inferastructure
{
    public static class DatabaseInitializer
    {
        public static void Initialize(BnanEGContext context, string seedDataSql)
        {
            if (context.Database.EnsureCreated())
            {
                // Database was created, seed data
                context.Database.ExecuteSqlRaw(seedDataSql);
            }
        }
    }
}

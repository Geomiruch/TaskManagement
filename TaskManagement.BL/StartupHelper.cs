using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.DAL;

namespace TaskManagement.BL
{
    public static class ServiceCollectionExtension
    {
        public static void RegisterDbContext(this IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddDbContext<ApplicationDbContext>(db => db.UseNpgsql(connectionString));
        }
    }
}

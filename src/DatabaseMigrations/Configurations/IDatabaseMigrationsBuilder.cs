using Microsoft.Extensions.DependencyInjection;

namespace DatabaseMigrations.Configurations
{
    public interface IDatabaseMigrationsBuilder
    {
        IServiceCollection Services { get; }
    }

    public class DatabaseMigrationsBuilder : IDatabaseMigrationsBuilder
    {
        public IServiceCollection Services { get; }

        public DatabaseMigrationsBuilder(IServiceCollection services)
        {
            this.Services = services;
        }
    }
}

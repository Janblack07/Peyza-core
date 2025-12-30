using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Peyza.Core.NotificationManagement.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peyza.Core.EntityFrameworkCore
{
    public class NotificationManagementDbContextFactory : IDesignTimeDbContextFactory<NotificationManagementDbContext>
    {
        public NotificationManagementDbContext CreateDbContext(string[] args)
        {
            // Lee la cadena desde el appsettings del Host
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "host", "Peyza.Core.HttpApi.Host");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<NotificationManagementDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new NotificationManagementDbContext(optionsBuilder.Options);
        }
    }
}

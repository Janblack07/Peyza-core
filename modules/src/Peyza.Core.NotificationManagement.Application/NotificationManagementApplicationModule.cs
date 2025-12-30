using Microsoft.Extensions.DependencyInjection;
using Peyza.Core.NotificationManagement.Providers;
using Peyza.Core.NotificationManagement.Providers.SendGrid;
using Peyza.Core.NotificationManagement.Workers;
using System;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.BackgroundWorkers;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(NotificationManagementDomainModule),
    typeof(NotificationManagementApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBackgroundWorkersModule)
    )]
public class NotificationManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<NotificationManagementApplicationModule>();

        var configuration = context.Services.GetConfiguration();

        context.Services.Configure<SendGridOptions>(configuration.GetSection("SendGrid"));

        context.Services.AddHttpClient(SendGridNotificationProviderDispatcher.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://api.sendgrid.com");
        });
        context.Services.Configure<NotificationDispatcherOptions>(
        configuration.GetSection("NotificationManagement:Dispatcher"));

        // Puerto -> Adaptador
        context.Services.AddTransient<INotificationProviderDispatcher, SendGridNotificationProviderDispatcher>();
    }
}

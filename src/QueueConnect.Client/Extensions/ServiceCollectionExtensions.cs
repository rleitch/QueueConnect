using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QueueConnect.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceBroker(this IServiceCollection services, string optionsSectionKey = null)
        {
            if (string.IsNullOrWhiteSpace(optionsSectionKey))
            {
                optionsSectionKey = nameof(QueueClientOptions);
            }

            services.AddOptions<QueueClientOptions>().Configure<IConfiguration>((options, configuration) => configuration.GetSection(optionsSectionKey).Bind(options));
            services.AddTransient<IQueueClient, QueueClient>();
            var serviceBrokerSettings = services.BuildServiceProvider().GetService<IOptions<QueueClientOptions>>().Value;

            services.AddDistributedSqlServerCache(o =>
            {
                o.ConnectionString = serviceBrokerSettings.ConnectionString;
                o.SchemaName = "dbo";
                o.TableName = "DistributedCache";
            });
            return services;
        }
    }
}

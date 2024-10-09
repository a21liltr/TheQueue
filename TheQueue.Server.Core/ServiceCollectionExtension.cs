using Microsoft.Extensions.DependencyInjection;
using TheQueue.Server.Core.Services;

namespace TheQueue.Server.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(
            this IServiceCollection services)
        {
            services.AddSingleton<QueueService>();
            services.AddSingleton<ClientService>();
            services.AddSingleton<SupervisorService>();
            services.AddSingleton<StudentService>();

            services.AddHostedService<PublishService>();
            services.AddHostedService<ReplyService>();

            return services;
        }
    }
}
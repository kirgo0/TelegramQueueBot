using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramQueueBot.Helpers
{
    public class HangfireActivator : JobActivator
    {
        private readonly IServiceScopeFactory _container;

        public HangfireActivator(IServiceScopeFactory container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            using var scope = _container.CreateScope();

            var b = scope.ServiceProvider.GetRequiredService(type);

            return scope.ServiceProvider.GetRequiredService(type);

        }
    }

}

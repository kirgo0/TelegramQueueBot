using Autofac;
using System.Reflection;
using TelegramQueueBot.Helpers.Attributes;
using TelegramQueueBot.UpdateHandlers;
using TelegramQueueBot.UpdateHandlers.Abstractions;

namespace TelegramQueueBot.Modules
{
    public class HandlersModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultUpdateHandler>();

            var assembly = Assembly.GetExecutingAssembly();

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttributes<HandlerMetadataAttribute>().Any());

            foreach (var handlerType in handlerTypes)
            {
                var metadata = handlerType.GetCustomAttribute<HandlerMetadataAttribute>();
                builder.RegisterType(handlerType)
                    .As<UpdateHandler>()
                    .WithMetadata(metadata.Key, metadata.Value);
            }

        }
    }
}

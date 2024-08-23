using Autofac;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using TelegramQueueBot.UpdateHandlers.Callbacks;

namespace TelegramQueueBot.Modules
{
    public class ActionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnqueueActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.Enqueue);

            builder.RegisterType<DequeueActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.Dequeue);

            builder.RegisterType<QueueMenuActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.QueueMenu);
        }
    }
}

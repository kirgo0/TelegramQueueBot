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

            builder.RegisterType<QueueListActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.QueueList);

            builder.RegisterType<QueueMenuActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.QueueMenu);

            builder.RegisterType<LoadActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.Load);

            builder.RegisterType<DeleteActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.Delete);

            builder.RegisterType<ConfirmDeletionActionHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleAction, Actions.ConfirmDeletion);
        }
    }
}

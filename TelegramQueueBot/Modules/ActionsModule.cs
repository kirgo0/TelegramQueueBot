using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}

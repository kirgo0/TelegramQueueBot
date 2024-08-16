using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramQueueBot.Common;
using TelegramQueueBot.UpdateHandlers.Abstractions;
using TelegramQueueBot.UpdateHandlers.Commands;

namespace TelegramQueueBot.Modules
{
    public class CommandsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Start);

            builder.RegisterType<GetCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Get);

            builder.RegisterType<CreateCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Create);

            builder.RegisterType<DefaultSizeCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.SetDefaultSize);

            builder.RegisterType<SaveCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Save);

            builder.RegisterType<SkipFirstCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.SkipFirst);

            builder.RegisterType<NotifyModeCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.NotifyMode);
        }
    }
}

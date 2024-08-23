using Autofac;
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

            builder.RegisterType<SizeCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.SetDefaultSize);

            builder.RegisterType<SaveCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Save);

            builder.RegisterType<SkipFirstCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.SkipFirst);

            builder.RegisterType<ModeCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.Mode);

            builder.RegisterType<LineupCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.LineUp);

            builder.RegisterType<SavedListCommandHandler>()
                .As<UpdateHandler>()
                .WithMetadata(Metatags.HandleCommand, Commands.SavedList);
        }
    }
}

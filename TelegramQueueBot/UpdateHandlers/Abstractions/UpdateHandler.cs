using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Extensions.Logging;
using QueueCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramQueueBot.Common;
using TelegramQueueBot.DataAccess.Abstraction;
using TelegramQueueBot.Models;
using TelegramQueueBot.UpdateHandlers.Commands;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UpdateHandler
    {
        protected ITelegramBotClient _bot;
        protected ILifetimeScope _scope;
        protected ILogger _log;
        public bool GroupsOnly { get; set; } = false;
        public bool CheckUserExists { get; set; } = false;
        public bool CheckChatExists { get; set; } = false;
        protected UpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger)
        {
            _bot = bot;
            _scope = scope;
            _log = logger;
        }

        public abstract Task Handle(Update update);

        protected async Task HandleWithChecks(Update update)
        {
            // check if the current update was requested from the group chat
            var isGroup = IsGroup(update, out long chatId);

            // return if the update should be processed only from the group chat request
            if (GroupsOnly && !isGroup) return;

            if (CheckUserExists)
                await TryCreateUser(update);

            if (isGroup && CheckChatExists)
                await TryCreateChat(chatId);

            await Handle(update);
        }

        public virtual async Task RedirectHandle(
            Update update, 
            string serviceMetaTag, 
            Func<Update, object, Meta<UpdateHandler>, bool> comparator, 
            string resolvingErrorMessage, 
            params object[] resolveErrorParams
            )
        {
            UpdateHandler handler = null;
            try
            {
                object value;
                var handlers = _scope.Resolve<IEnumerable<Meta<UpdateHandler>>>();
                foreach (var item in handlers)
                {
                    if (item.Metadata.TryGetValue(serviceMetaTag, out value))
                    {
                        if(comparator.Invoke(update, value, item))
                        {
                            handler = item.Value;
                            break;
                        }
                        if (handler is not null) break;
                    }
                }
                if (handler is null) throw new Exception();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, resolvingErrorMessage, resolveErrorParams);
                return;
            }
            try
            {
                await handler.HandleWithChecks(update);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while handling a request with a  {handler}", handler);
                return;
            }
        }

        private bool IsGroup(Update update, out long id)
        {
            id = 0;
            if (update?.Message?.Chat is not null)
            {
                id = update.Message.Chat.Id;
            }
            if (update?.CallbackQuery?.Message?.Chat is not null)
            {
                id = update.CallbackQuery.Message.Chat.Id;
            }
            return id < 0;
        }

        private async Task TryCreateChat(long chatId)
        {
            var chats = _scope.Resolve<IChatRepository>();
            if(await chats.GetByTelegramIdAsync(chatId) is null)
            {
                var chat = new Models.Chat(chatId);
                await chats.CreateAsync(chat);
            }
        }

        private async Task TryCreateUser(Update update)
        {
            var users = _scope.Resolve<IUserRepository>();
            Telegram.Bot.Types.User from = new();
            if (update?.Message?.From is not null)
            {
                from = update.Message.From;
            }
            if (update?.CallbackQuery?.From is not null)
            {
                from = update.CallbackQuery.From;
            }

            if (await users.GetByTelegramIdAsync(from.Id) is null)
            {
                var user = new Models.User(from.Id, from.Username);
                await users.CreateAsync(user);
            }
        }
    }

}

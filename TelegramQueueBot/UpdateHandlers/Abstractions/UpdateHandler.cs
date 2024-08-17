using Autofac;
using Autofac.Features.Metadata;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Update = Telegram.Bot.Types.Update;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.UpdateHandlers.Abstractions
{
    public abstract class UpdateHandler
    {
        protected ITelegramBotClient _bot;
        protected ILifetimeScope _scope;
        protected ILogger _log;
        protected IUserRepository _userRepository;
        protected IChatRepository _chatRepository;
        public bool GroupsOnly { get; set; } = false;
        protected bool _isGroup;
        protected long _chatId;
        public bool NeedsUser { get; set; } = false;
        private Task<User> _userTask;
        protected Task<User> userTask
        {
            get
            {
                if (!NeedsUser)
                {
                    _log.LogWarning("Be careful, you are requesting the user without specifying the {name} attribute inside the {handler} constructor", nameof(NeedsUser), GetType().Name);
                    return Task.FromResult<User>(null);
                }
                return _userTask;
            }
            set
            {
                _userTask = value;
            }
        }
        public bool NeedsChat { get; set; } = false;

        private Task<Chat> _chatTask;

        protected Task<Chat> chatTask
        {
            get
            {
                if (!NeedsChat)
                {
                    _log.LogWarning("Be careful, you are requesting a chat without specifying the {name} attribute inside the {handler} constructor", nameof(NeedsChat), GetType().Name);
                    return Task.FromResult<Chat>(null);
                }
                return _chatTask;
            }
            set
            {
                _chatTask = value;
            }
        }
        protected UpdateHandler(ITelegramBotClient bot, ILifetimeScope scope, ILogger logger)
        {
            _bot = bot;
            _scope = scope;
            _log = logger;
        }

        public abstract Task Handle(Update update);

        protected async Task PrepareAndHandle(
            Update update,
            Task<User> userTask = null,
            Task<Chat> chatTask = null,
            bool isGroup = false,
            long chatId = 0
            )
        {
            // check if the current update was requested from the group chat
            if (chatId != 0)
            {
                _isGroup = isGroup;
                _chatId = chatId;
            }
            else _isGroup = IsGroup(update, out _chatId);

            // return if the update should be processed only from the group chat request
            if (GroupsOnly && !_isGroup) return;

            if (NeedsUser && userTask is null)
            {
                _userRepository = _scope.Resolve<IUserRepository>();
                if (userTask is null) _userTask = TryGetOrCreateUser(update);
                else _userTask = userTask;
            }

            if (_isGroup && NeedsChat)
            {
                _chatRepository = _scope.Resolve<IChatRepository>();
                if (chatTask is null) _chatTask = TryGetOrCreateChat(chatId);
                else _chatTask = chatTask;
            }

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
                    if (!item.Metadata.TryGetValue(serviceMetaTag, out value))
                        continue;
                    if (comparator.Invoke(update, value, item))
                    {
                        handler = item.Value;
                        break;
                    }
                    if (handler is not null) break;
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
                await handler.PrepareAndHandle(update, _userTask, _chatTask, _isGroup, _chatId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while handling a request with the {handler}", handler);
                return;
            }
        }

        protected string GetAction(Update update)
        {
            if(update?.CallbackQuery?.Data is null)
            {
                _log.LogWarning("Update has no callback data in {type}", GetType());
            }
            return update.CallbackQuery.Data;
        }

        protected IEnumerable<string> GetArguments(Update update)
        {
            var parts = update.Message.Text.Split(' ');
            return parts.Skip(1);
        }

        private bool IsGroup(Update update, out long id)
        {
            id = 0;
            if (update?.Message?.Chat is not null)
            {
                id = update.Message.Chat.Id;
            }
            else if (update?.CallbackQuery?.Message?.Chat is not null)
            {
                id = update.CallbackQuery.Message.Chat.Id;
            }
            return id < 0;
        }

        private async Task<Chat> TryGetOrCreateChat(long chatId)
        {
            var chat = await _chatRepository.GetByTelegramIdAsync(chatId);
            if (chat is null) chat = await _chatRepository.CreateAsync(new Chat(chatId));
            return chat;
        }

        private async Task<User> TryGetOrCreateUser(Update update)
        {
            Telegram.Bot.Types.User from = null;
            if (update?.Message?.From is not null)
                from = update.Message.From;
            else if (update?.CallbackQuery?.From is not null)
                from = update.CallbackQuery.From;

            if (from is null) throw new NullReferenceException("The sender of the request was unable to be identified");

            var user = await _userRepository.GetByTelegramIdAsync(from.Id);

            if (user is null)
            {
                user = await _userRepository.CreateAsync(new User(from.Id, from.Username));
            }
            return user;
        }
    }

}

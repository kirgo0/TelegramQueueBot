using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramQueueBot.Common;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Helpers
{
    public class MessageBuilder
    {
        private StringBuilder _messageText = new();
        private int _buttonsRow = 0;
        public long ChatId { get; set; }
        public int LastMessageId { get; set; }
        public string Text => _messageText.ToString();
        public List<List<InlineKeyboardButton>> Buttons = new();
        public InlineKeyboardMarkup ButtonsMarkup => new InlineKeyboardMarkup(Buttons);
        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        public static async Task<MessageBuilder> CreateAsync(Chat chat, ITextRepository textRepository)
        {
            var builder = new MessageBuilder(chat);
            await builder.AppendModeTitle(chat, textRepository);
            return builder;
        }

        public async Task<MessageBuilder> AppendModeTitle(Chat chat, ITextRepository textRepository)
        {
            if (chat.Mode is Models.Enums.ChatMode.CallingUsers)
            {
                AppendTextLine(await textRepository.GetValueAsync(TextKeys.QueueIsCallingUsers));
                AppendTextLine();
            }
            return this;
        }

        public MessageBuilder() { }
        public MessageBuilder(Chat chat)
        {
            ChatId = chat.TelegramId;
            LastMessageId = chat.LastMessageId;
        }

        public MessageBuilder SetChatId(long chatId)
        {
            ChatId = chatId;
            return this;
        }
        public MessageBuilder SetLastMessageId(int lastMessageId)
        {
            LastMessageId = lastMessageId;
            return this;
        }
        public MessageBuilder SetParseMode(ParseMode parseMode)
        {
            ParseMode = parseMode;
            return this;
        }

        public MessageBuilder AppendText(string text)
        {
            _messageText.Append(text);
            return this;
        }

        public MessageBuilder AppendTextLine(string text)
        {
            _messageText.AppendLine(text);
            return this;
        }
        public MessageBuilder AppendTextLine()
        {
            _messageText.AppendLine(string.Empty);
            return this;
        }

        public MessageBuilder AddButton(string text, string callbackData = "", string url = "")
        {
            if (string.IsNullOrWhiteSpace(callbackData) && string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("At least one callbackValue or url parameter must be specified");

            var button = new InlineKeyboardButton(text);

            if (!string.IsNullOrWhiteSpace(callbackData))
                button.CallbackData = callbackData;
            if (!string.IsNullOrWhiteSpace(url))
                button.Url = url;

            if (Buttons.ElementAtOrDefault(_buttonsRow) is null) Buttons.Add(new());
            Buttons[_buttonsRow].Add(button);

            return this;
        }

        public MessageBuilder AddButtonNextRow(string text, string callbackData = "", string url = "")
        {
            AddButton(text, callbackData, url);
            _buttonsRow++;
            return this;
        }

    }
}

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramQueueBot.Models;

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

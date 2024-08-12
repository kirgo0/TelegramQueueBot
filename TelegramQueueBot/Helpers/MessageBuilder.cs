using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramQueueBot.Helpers
{
    public class MessageBuilder
    {
        private StringBuilder _messageText = new();
        private int _buttonsRow = 0;
        public int ChatId { get; set; }
        public string Text => _messageText.ToString();
        public List<List<InlineKeyboardButton>> Buttons = new();
        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        public MessageBuilder() { }

        public MessageBuilder SetChatId(int chatId)
        {
            ChatId = chatId;
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

    }
}

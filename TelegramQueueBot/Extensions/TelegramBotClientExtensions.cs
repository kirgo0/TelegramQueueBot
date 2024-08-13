using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Helpers;

namespace TelegramQueueBot.Extensions
{
    public static class TelegramBotClientExtensions
    {

        public static async Task<Message> BuildAndSendAsync(this ITelegramBotClient bot, MessageBuilder builder)
        {
            return await bot.SendTextMessageAsync(
                builder.ChatId, 
                builder.Text, 
                parseMode: builder.ParseMode,
                replyMarkup: builder.ButtonsMarkup
                );
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramQueueBot.Helpers;
using TelegramQueueBot.Repository.Interfaces;
using TelegramQueueBot.Services;

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

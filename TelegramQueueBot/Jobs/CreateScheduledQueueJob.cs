using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Jobs
{
    public class CreateScheduledQueueJob
    {
        private readonly ITelegramBotClient _bot;
        private readonly IChatRepository _chatRepository;
        private readonly ITextRepository _textRepository;
        private readonly ILogger<CreateScheduledQueueJob> _log;

        public CreateScheduledQueueJob(ITelegramBotClient bot, IChatRepository chatRepository, ILogger<CreateScheduledQueueJob> log, ITextRepository textRepository)
        {
            _chatRepository = chatRepository;
            _textRepository = textRepository;
            _bot = bot;
            _log = log;
        }

        public async Task ExecuteJob(long chatId, string? queueId = null)
        {
            //var chat = await _chatRepository.GetByTelegramIdAsync(chatId);
            //if (chat is null) {
            //    _log.LogWarning("TODO warning CreateScheduledQueueJob");
            //    return;
            //}
            //var msg = new MessageBuilder(chat)
            //    .AppendText(await _textRepository.GetValueAsync(TextKeys.JobCreatedQueue));
        }
    }
}

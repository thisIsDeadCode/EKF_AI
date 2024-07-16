using EKF_AI.DataBase;
using Quartz;
using Telegram.Bot;

namespace BotTG
{
    public class TelegramNotification : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var contextDb = new ApplicationContext();

            contextDb.Images.Where(x => x.HasSend == false).ToList().ForEach(async x =>
            {
                if (!string.IsNullOrEmpty(x.Result))
                {
                    var bot = new TelegramBotClient("7312902127:AAGpXHIoFO-By8KGk-YCQzgY8gq-Ugundnc");

                    await bot.SendTextMessageAsync(x.ChatId,
                        "Картинка обработана." +
                        $"\n{x.Id}" +
                        $"\n{x.Name}" +
                        $"\n" +
                        $"\n{x.Result}" +
                        $"\nТочность: {x.Precision}");
                    x.HasSend = true;
                    x.HasProcessed = true;
                    contextDb.SaveChanges();
                }
            });
        }
    }
}


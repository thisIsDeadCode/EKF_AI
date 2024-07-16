using BotTG;
using EKF_AI.DataBase;
using EKF_AI.DataBase.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;

var token = "7312902127:AAGpXHIoFO-By8KGk-YCQzgY8gq-Ugundnc";
//var context = new ApplicationContext();


var bot = new TelegramBotClient(token);
var me = await bot.GetMeAsync();
bot.StartReceiving(OnUpdate, OnError);


Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

async Task OnError(ITelegramBotClient client, Exception exception, CancellationToken ct)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, ct);
}

async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    if(update.Message.Photo == null)
    {
        return;
    }

    var test = await bot.GetFileAsync(update.Message.Photo[update.Message.Photo.Count() - 1].FileId);
    var download_url = @$"https://api.telegram.org/file/bot{token}/{ test.FilePath}";
    var extension = test.FilePath.Split('.').Last();
    var imageId = Guid.NewGuid().ToString();
    var base64 = string.Empty;  


    using (HttpClient httpClient = new HttpClient())
    {
        var response = await httpClient.GetAsync(new Uri(download_url));

        using (var fs = new FileStream(@$"D:\temp\{imageId}.{extension}", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            await response.Content.CopyToAsync(fs);
            base64 = ConvertToBase64(fs);
        }
    }

    var image = new Image
    {
        ChatId = update.Message.Chat.Id,
        HasSend = false,
        Id = imageId,
        Path = @$"D:\temp\{imageId}.{extension}",
        HasProcessed = false
    };

    //context.Images.Add(image);
    //context.SaveChanges();

    await bot.SendTextMessageAsync(update.Message.Chat, "Картинку сохранил");

    var result = await ExecutePython($"{image.Id}.{ extension}");

    if (result == null)
    {
        await bot.SendTextMessageAsync(update.Message.Chat, "Ошибка при обработке изображения");
        return;
    }

    string message = $@"Количество найденных элементов: {result.Count()}
Результат:";

    foreach(var item in result)
    {
        message += $"\n{item.Key}: {item.Value}";
    }


    using (StreamReader sr = new StreamReader(@$"D:\temp\result_{imageId}.{extension}"))
    {
        await bot.SendDocumentAsync(update.Message.Chat, InputFile.FromStream(sr.BaseStream));
    }

    await bot.SendTextMessageAsync(update.Message.Chat, message);

}

static string ConvertToBase64(Stream stream)
{
    byte[] bytes;
    using (var memoryStream = new MemoryStream())
    {
        stream.CopyTo(memoryStream);
        bytes = memoryStream.ToArray();
    }

    string base64 = Convert.ToBase64String(bytes);
    return base64;
}


static async Task<Dictionary<string, int>> ExecutePython(string imgName)
{
    string pythonPath = @"C:\Users\PCZONE.GE\AppData\Local\Programs\Python\Python39\python.exe";
    string scriptPath = @"C:\Users\PCZONE.GE\source\repos\EKF_AI\ObjectDetectionPy\ObjectDetection.py";

    if (!System.IO.File.Exists(pythonPath))
    {
        Console.WriteLine("Python executable not found.");
        return null;
    }

    if (!System.IO.File.Exists(scriptPath))
    {
        Console.WriteLine("Python script not found.");
        return null;
    }

    ProcessStartInfo start = new ProcessStartInfo
    {
        FileName = pythonPath,
        Arguments = $"\"{scriptPath}\" \"{imgName}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    try
    {
        using (Process process = Process.Start(start))
        {
            // Чтение стандартного вывода
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            // Чтение ошибок
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            // Ожидание завершения процесса
            await Task.WhenAll(outputTask, errorTask);

            string output = outputTask.Result;
            string errors = errorTask.Result;

            // Если есть ошибки, выбрасываем исключение с текстом ошибки
            if (!string.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }

            var result = JsonConvert.DeserializeObject<Dictionary<string, int>>(output);
            return result;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        return null;
    }
}

public class Element
{
    public string Name { get; set; }
    public double Count { get; set; }
}
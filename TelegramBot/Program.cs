using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class Program
    {
        static ITelegramBotClient botClient;
        static List<Tuple<Chat, DateTime, string>> tasks_list = new List<Tuple<Chat, DateTime, string>>();

        static void Main()
        {
            botClient = new TelegramBotClient("YOUR_ACCESS_TOKEN_HERE");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            DoRemind();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();
        }

        static async void DoRemind()
        {
            // in task_list:
            // Item1 is Chat
            // Item2 is Date and time
            // Item3 is the text(task) 

            while (true) // checks all the time
            {
                foreach (var task in tasks_list)
                {
                    if (task.Item2 <= DateTime.Now)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: task.Item1,
                            text: $"It`s time to do {task.Item3} !\n"
                             );

                        tasks_list.Remove(task);
                    }
                }

                Thread.Sleep(1000); // here we reduce the frequency of checks(to prevent computer do too much work)
            }               
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}: {e.Message.Text}");

                // RULE OF USAGE:
                // Date time
                // The message

                var data = e.Message.Text.Split("\n");

                if (data.Length != 2)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "Ouh, what`s that?\n"
                         );

                    return;
                }

                if (DateTime.TryParseExact(data[0], "yyyy-MM-dd HH-mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    tasks_list.Add(Tuple.Create(e.Message.Chat, dt, data[1]));

                    await botClient.SendTextMessageAsync(
                       chatId: e.Message.Chat,
                       text: $"Got it, will remind you about {data[1]}, at {data[0]} \n"
                        );
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                       chatId: e.Message.Chat,
                       text: "Date is not valid =(\n"
                        );
                    return;
                }
            }
        }
    }
}

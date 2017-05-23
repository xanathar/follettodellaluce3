using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace fdl3
{
    class TelegramNotificationService : IService
    {
        TelegramBotClient Bot = new TelegramBotClient(Config.TelegramApiToken);
        int m_Errors = 0;

        HashSet<long> m_Subscribers;

        public string GetName()
        {
            return "telegram_bot";
        }

        public string GetStatus()
        {
            return $"{m_Errors} since last restart.";
        }

        public void OnMessage(DateTime time, PowerEvent pe)
        {
            if (pe.IsImportant())
            {
                foreach (long sub in m_Subscribers)
                {
                    try
                    {
                        Bot.SendTextMessageAsync(sub, $"New event : {pe}");
                    }
                    catch (Exception ex)
                    {
                        Handle(ex);
                    }
                }
            }
        }

        public void Start()
        {
            try
            {
                if (System.IO.File.Exists(Path.Combine(Config.WorkPath, "subscribers")))
                {
                    m_Subscribers = new HashSet<long>(System.IO.File.ReadAllLines(Path.Combine(Config.WorkPath, "subscribers"))
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => long.Parse(s)));
                }
                else
                    m_Subscribers = new HashSet<long>();

                Bot.OnMessage += Bot_OnMessage;
                Bot.OnReceiveError += Bot_OnReceiveError;
                Bot.OnReceiveGeneralError += Bot_OnReceiveGeneralError;
                Bot.StartReceiving();
            }
            catch (Exception ex)
            {
                Handle(ex);
            }
        }

        private void Bot_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            Handle(e.Exception);
        }

        private void Bot_OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Handle(e.ApiRequestException);
        }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Type != MessageType.TextMessage)
                    SendHelp(e.Message.Chat.Id, true);

                if (e.Message.Text == "subscribe")
                {
                    if (m_Subscribers.Add(e.Message.Chat.Id))
                    {
                        System.IO.File.WriteAllLines(Path.Combine(Config.WorkPath, "subscribers"), m_Subscribers.Select(l => l.ToString()).ToArray());
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, "DONE!");
                    }
                    else
                    {
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, "Already subscribed.");
                    }
                }
                else if (e.Message.Text == "subscribers")
                {
                    StringBuilder sb = new StringBuilder();
                    foreach(long l in m_Subscribers)
                    {
                        var chat = Bot.GetChatAsync(l).Do();
                        sb.AppendLine($"{l} : {chat.Username} ({chat.FirstName} {chat.LastName})");
                    }
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, sb.ToString());
                }
                else if (e.Message.Text == "status")
                {
                    foreach (var svc in Services.All)
                    {
                        var status = svc.GetStatus();
                        var name = svc.GetName();
                        StringBuilder sb = new StringBuilder();

                        if (status != null)
                        {
                            sb.AppendLine($"{name} : {status}");
                        }
                        Bot.SendTextMessageAsync(e.Message.Chat.Id, sb.ToString());
                    }
                }
                else if (e.Message.Text == "log")
                {
                    Bot.SendTextMessageAsync(e.Message.Chat.Id, string.Join("\n", Services.Logger().GetLast10Entries()));
                }
                else if (e.Message.Text == "dlog")
                {
                    Stream stream = new MemoryStream(System.IO.File.ReadAllBytes(Services.Logger().GetFile()));
                    Bot.SendDocumentAsync(e.Message.Chat.Id, new FileToSend("log" + Guid.NewGuid().ToString("n"), stream)).Do();
                }
                else if (e.Message.Text == "help" || e.Message.Text == "?")
                {
                    SendHelp(e.Message.Chat.Id, false);
                }
                else
                {
                    SendHelp(e.Message.Chat.Id, true);
                }
            }
            catch (Exception ex)
            {
                Handle(ex);
            }
        }

        private void Handle(Exception ex)
        {
            m_Errors += 1;

            Services.Broadcast(this, DateTime.Now, new PowerEvent()
            {
                Error = ex,
                Event = EventKind.UnknownError
            });
        }

        private void SendHelp(long id, bool isError)
        {
            StringBuilder sb = new StringBuilder();

            if (isError)
                sb.AppendLine("Syntax error! :(");


            sb.AppendLine("Available commands : ");
            sb.AppendLine("   subscribe - subscribe to notifications");
            sb.AppendLine("   subscribers - show current subscribers");
            sb.AppendLine("   status  - get status");
            sb.AppendLine("   log  - get log (last 10 rows)");
            sb.AppendLine("   dlog  - get log (as attachment)");

            Bot.SendTextMessageAsync(id, sb.ToString());
        }
    }
}

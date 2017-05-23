#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fdl3
{
    public static class Config
    {
        public const string WorkPath = @"-- PATH WHERE TO STORE LOG FILES AND LIST OF SUBSCRIBED USERS --";
        public const string PollUrl = @"-- URL TO POLL, GET METHOD, PLAIN HTTP --";
        public const string TelegramApiToken = @"-- TELEGRAM API TOKEN --";
        public const string StringToFind = "-- STRING TO BE FOUND IN FETCHED CONTENT --";
        public const int IntervalInMinutes = 1; // minutes of polling interval

    }
}

#endif

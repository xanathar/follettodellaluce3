using System;
using System.Threading;

namespace fdl3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("folletto-della-luce 3.0");

            Services.Add(new LogService());
            Services.Add(new TelegramNotificationService());
            Services.Add(new WatchDogService());

            foreach (var s in Services.All)
            {
                Console.Write("Starting {0}...", s.GetName());
                s.Start();
                Console.WriteLine("done.");
            }

            ManualResetEvent mre = new ManualResetEvent(false);
            mre.WaitOne();
        }
    }
}

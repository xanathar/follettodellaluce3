using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fdl3
{
    public class LogService : IService
    {
        int m_Lines = 0;
        int m_Errors = 0;
        DateTime m_StartTime = DateTime.UtcNow;

        List<string> m_Last10Rows = new List<string>();

        public string GetName()
        {
            return "logger";
        }

        public string GetStatus()
        {
            return $"{m_Lines} logged, of which {m_Errors} were errors. Uptime : {DateTime.UtcNow - m_StartTime}.";
        }

        public void OnMessage(DateTime time, PowerEvent pe)
        {
            m_Lines += 1;

            if (pe.Event == EventKind.UnknownError)
                m_Errors += 1;

            string msg = $"{time.ToString("u")} : {pe}";

            File.AppendAllText(GetFile(), $"{msg}\n");

            Console.WriteLine(msg);

            m_Last10Rows.Add(msg);

            if (m_Last10Rows.Count > 10)
                m_Last10Rows.RemoveAt(0);
        }

        public string GetFile()
        {
            return Path.Combine(Config.WorkPath, "logfile");
        }

        public string[] GetLast10Entries()
        {
            return m_Last10Rows.ToArray();
        }

        public void Start()
        {
            File.AppendAllText(GetFile(), "===========================================================\n");
        }
    }
}

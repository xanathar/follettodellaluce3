using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace fdl3
{
    public class WatchDogService : IService
    {
        bool? m_LastState = null;
        int m_Success = 0;
        int m_Totals = 0;
        DateTime m_LastPing = DateTime.MinValue;
        Timer m_Timer;

        public string GetName()
        {
            return "watchdog";
        }

        private string GetSummary()
        {
            if (m_LastState.HasValue)
                return m_LastState.Value ? "power is ON" : "power is OFF";

            return "unknown";
        }

        public string GetStatus()
        {
            decimal rate = 0.0m;

            if (m_Totals > 0)
                rate = Math.Round((decimal)m_Success / (decimal)m_Totals * 100.0m, 2);

            return $"{GetSummary()} - {m_Success}/{m_Totals} {rate}% - last at {m_LastPing.ToString("u")}";
        }

        public void OnMessage(DateTime time, PowerEvent pe)
        {
        }

        public void Start()
        {
            m_Timer = new Timer(o => OnTimerTick(), null, 1000, Config.IntervalInMinutes * 60 * 1000);
        }

        string HttpGet(string url)
        {
            string html = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Do())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        html = reader.ReadToEnd();
                    }
                }
            }

            return html;
        }

        void OnTimerTick()
        {
            try
            {
                string s = HttpGet(Config.PollUrl);

                if (!s.Contains(Config.StringToFind))
                    throw new Exception("String pattern not matched.");
                
                ReportPollState(true, null);
            }
            catch(Exception ex)
            {
                ReportPollState(false, ex);
            }
        }

        void ReportTransient(bool powerstate, Exception ex)
        {
            Services.Broadcast(this, m_LastPing, new PowerEvent()
            {
                Event = powerstate ? EventKind.PowerBack : EventKind.PowerOut,
                Error = ex
            });
        }

        void ReportPolling(bool powerstate, Exception ex)
        {
            Services.Broadcast(this, m_LastPing, new PowerEvent()
            {
                Event = powerstate ? EventKind.PowerStillBack : EventKind.PowerStillOut,
                Error = ex
            });
        }

        void ReportPollState(bool state, Exception ex)
        {
            m_LastPing = DateTime.Now;

            m_Totals += 1;

            if (state)
                m_Success += 1;

            if (m_LastState == null)
            {
                m_LastState = state;
                ReportPolling(state, ex);
                return;
            }

            if (m_LastState.Value != state)
            {
                ReportTransient(state, ex);
            }
            else
            {
                ReportPolling(state, ex);
            }

            
            m_LastState = state;
        }
    }
}

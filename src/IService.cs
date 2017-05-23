using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fdl3
{
    public interface IService
    {
        void OnMessage(DateTime time, PowerEvent pe);
        void Start();
        string GetStatus();
        string GetName();
    }

    public class Services
    {
        private static List<IService> m_Services = new List<IService>();

        public static LogService Logger()
        {
            return m_Services.OfType<LogService>().Single();
        }

        public static void Add(IService svc)
        {
            m_Services.Add(svc);
        }

        public static void Broadcast(IService source, DateTime time, PowerEvent pe)
        {
            foreach (var s in m_Services.Where(s => s != source))
                s.OnMessage(time, pe);
        }

        public static IEnumerable<IService> All
        {
            get { return m_Services; }
        }
    }
}

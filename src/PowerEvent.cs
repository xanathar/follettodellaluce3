using System;

namespace fdl3
{
    public class PowerEvent
    {
        public EventKind Event;
        public Exception Error;

        public bool IsImportant()
        {
            return Event == EventKind.PowerOut || Event == EventKind.PowerBack || Event == EventKind.UnknownError;
        }

        public override string ToString()
        {
            Exception ex = Error;

            if (ex is AggregateException)
                ex = ((AggregateException)ex).InnerExceptions[0];

            if (ex != null)
                return $"{Event} - {ex.Message}";
            else
                return Event.ToString();
        }
    }
}
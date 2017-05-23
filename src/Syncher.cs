using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fdl3
{
    public static class Syncher
    {
        public static T Do<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}

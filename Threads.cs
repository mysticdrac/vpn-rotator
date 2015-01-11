using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace vpnrotator
{
    internal class Threads
    {
        Form1 Parent;
        Process p;
        ProcessStartInfo s;
        internal Threads(Form1 f) {
            Parent = f;
        }

        internal void wait(object obj)
        {
            int timewait = (int)obj;
            Thread.Sleep(timewait);
            Parent.setevent();

        }
    }
}

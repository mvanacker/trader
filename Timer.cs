using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader
{
    public class Timer : BackgroundLoop
    {
        public event EventHandler TimedOut;

        private DateTime LastReset { get; set; } = DateTime.Now;

        public void Reset() => LastReset = DateTime.Now;
        protected override void Iterate()
        {
            if (DateTime.Now - LastReset > Const.GENERAL_SOCKET_TIMEOUT)
            {
                TimedOut(this, EventArgs.Empty);
                Reset();
            }
        }
    }
}

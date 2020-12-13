using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace Trader
{
    public abstract class BackgroundLoop : BackgroundWorker
    {
        public bool IsWorking { get; protected set; }
        public int Timeout { get; protected set; }

        public event EventHandler Iterated;

        protected virtual void OnIterated(EventArgs e) => Iterated?.Invoke(this, e);

        public BackgroundLoop()
        {
            WorkerSupportsCancellation = true;
            DoWork += Work;
            IsWorking = false;
            Timeout = Const.STANDARD_TIMEOUT;
        }

        protected abstract void Iterate();
        protected void Work(object sender, DoWorkEventArgs e)
        {
            IsWorking = true;
            while (IsWorking)
            {
                Iterate();
                OnIterated(EventArgs.Empty);
                Thread.Sleep(Timeout);
            }
        }
        public new virtual void CancelAsync()
        {
            IsWorking = false;
            base.CancelAsync();
        }
    }
}

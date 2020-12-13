using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using System.Threading;
using System.Data.SqlClient;

namespace Trader
{
    using Database;

    public abstract class SubscribedSocket : SecureWebSocket
    {
        public SubscribedSocket(string url) : this(url, string.Empty, string.Empty) { }
        public SubscribedSocket(string url, string key, string secret) : base(url, key, secret)
        {
            OnMessage += Receive;
            OnError += (s, e) => Close();
            OnOpen += (s, e) =>
            {
                Console.WriteLine($"{Name} connected.");
                StartTime = DateTime.Now;
                if (!string.IsNullOrEmpty(Key) && !string.IsNullOrEmpty(Secret))
                {
                    Status = SocketStatus.Authenticating;
                    Send(Authentication);
                }
                else Subscribe();
            };
            OnClose += (s, e) =>
            {
                Console.WriteLine($"{Name} disconnected. Clean: {e.WasClean}. {e.Code} {e.Reason}");
                Status = SocketStatus.Connecting;
            };
            Timer.TimedOut += (s, e) =>
            {
                if (IsAlive)
                {
                    Console.WriteLine($"{Name} connection timed out after {Const.GENERAL_SOCKET_TIMEOUT.TotalMilliseconds} ms.");
                    Close();
                }
            };
            Timer.RunWorkerAsync();
        }

        private Timer Timer { get; } = new Timer();

        public string Name { get; set; }
        public IConverter Converter { get; set; }
        public SocketStatus Status { get; private set; } = SocketStatus.Connecting;
        public List<Subscription> Subscriptions { get; } = new List<Subscription>();
        public DateTime StartTime { get; private set; } = DateTime.Now;
        protected HashSet<string> ExcludedTables { get; } = new HashSet<string>();

        public event EventHandler Subscribed;
        public event EventHandler<TableEventArgs> TableCreated, TableChanged;
        public event EventHandler<TickEventArgs> Tick;
        public event EventHandler<TradeBinEventArgs> TradeBinPartial;

        protected virtual void OnTableCreated(TableEventArgs e) => TableCreated?.Invoke(this, e);
        protected virtual void OnTableChanged(TableEventArgs e) => TableChanged?.Invoke(this, e);
        protected virtual void OnTick(TickEventArgs e) => Tick?.Invoke(this, e);
        protected virtual void OnTradeBinPartial(TradeBinEventArgs e) => TradeBinPartial?.Invoke(this, e);

        private void Receive(object sender, MessageEventArgs e)
        {
            Timer.Reset();
            var json = e.Data.ToJson();
            if (IsHeartbeat(json))
            {
                Send(Heartbeat);
            }
            else
            {
                switch (Status)
                {
                    case SocketStatus.Authenticating:
                        if (IsAuthenticated(json))
                        {
                            Console.WriteLine(e.Data);
                            Console.WriteLine($"{Name} authenticated.");
                            Subscribe();
                        }
                        break;
                    case SocketStatus.Subscribing:
                        Store(e.Data, json);
                        ConfirmSubscription(json);
                        if (SubscriptionsConfirmed)
                        {
                            Console.WriteLine($"{Name} subscribed.");
                            Stream();
                        }
                        break;
                    case SocketStatus.Streaming:
                        Store(e.Data, json);
                        break;
                }
            }
        }
        private void Subscribe()
        {
            if (SubscriptionsConfirmed) Stream();
            else
            {
                Status = SocketStatus.Subscribing;
                foreach (var subscription in Subscriptions)
                {
                    Console.WriteLine(subscription.ToString());
                    Send(subscription.ToString());
                }
                Task.Factory.StartNew(SubscriptionTimeout);
            }
        }
        private void SubscriptionTimeout()
        {
            Thread.Sleep(Const.GENERAL_SOCKET_TIMEOUT);
            if (IsAlive && !SubscriptionsConfirmed)
            {
                Console.WriteLine($"{Name} subscription timed out after {Const.GENERAL_SOCKET_TIMEOUT.TotalMilliseconds} ms");
                Close();
            }
        }
        private void Stream()
        {
            Subscribed(this, EventArgs.Empty);
            Status = SocketStatus.Streaming;
        }
        protected virtual bool IsHeartbeat(Json json) => false;
        protected virtual string Heartbeat { get; set; }
        protected abstract bool IsAuthenticated(Json json);
        protected virtual string Authentication { get; set; }
        public int Store(string data, Json json = null)
        {
            string sql = Converter.Convert(data);
            int rows = -1;
            if (!string.IsNullOrWhiteSpace(sql))
            {
                //Console.WriteLine(sql);
                try
                {
                    if (!ExcludedTables.Contains(json?.Table))
                    {
                        rows = Database.Execute(sql);
                    }
                    if (json != null) FireEvents(json);
                }
                catch (SqlException ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    if (IsAlive) Close();
                }
            }
            return rows;
        }
        protected virtual void FireEvents(Json json) { }
        protected abstract void ConfirmSubscription(Json json);
        private bool SubscriptionsConfirmed
        {
            get => Subscriptions.All(sub => sub.IsConfirmed());
        }
    }

    public class TableEventArgs : EventArgs
    {
        public TableEventArgs(string table, string label)
        {
            Table = table;
            Label = label;
        }

        public string Table { get; set; }
        public string Label { get; set; }
    }

    public class TickEventArgs : EventArgs
    {
        public TickEventArgs(Symbol symbol, double price)
        {
            Symbol = symbol;
            Price = price;
        }

        public Symbol Symbol { get; set; }
        public double Price { get; set; }
    }

    public class TradeBinEventArgs : EventArgs
    {
        public TradeBinEventArgs(TradeBin tradeBin, bool emptyPartial)
        {
            TradeBin = tradeBin;
            EmptyPartial = emptyPartial;
        }

        public TradeBin TradeBin { get; set; }
        public bool EmptyPartial { get; set; }
    }

    public enum SocketStatus
    {
        Connecting, Authenticating, Subscribing, Streaming
    }
}

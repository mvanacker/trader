using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Trader.Exchanges.Bitmex
{
    using Database;

    public class BitmexSocket : SubscribedSocket
    {
        public BitmexSocket(string url, string key, string secret) : base(url, key, secret)
        {
            ExcludedTables.Add("trade");
        }

        private string _lastAuth;
        protected override string Authentication
        {
            get
            {
                long nonce = Utils.Nonce;
                _lastAuth = $"{{\"op\":\"authKey\",\"args\":[\"{Key}\",{nonce},\"{Utils.Hmacsha256(Secret, "GET/realtime" + nonce)}\"]}}";
                return _lastAuth;
            }
        }

        protected override bool IsAuthenticated(Json json)
        {
            return json.Success is bool s && s && json.Request == _lastAuth.Replace("\"", string.Empty);
        }
        protected override void ConfirmSubscription(Json json)
        {
            var topic = json.Table;
            if (json.Action == Action.Partial)
            {
                var confirmations = Subscriptions
                    .Select(sub => sub.Confirmations)
                    .Single(con => con.ContainsKey(topic));
                confirmations[topic]++;
                Console.WriteLine($"{Name} confirmed {topic}.");
            }
        }
        protected override void FireEvents(Json json)
        {
            if (IsTradeBin(json)) FireTradeBinEvents(json);
            if (IsTicker(json)) FireTickEvents(json);
            FireTableEvents(json);
        }
        private bool IsTradeBin(Json json)
        {
            return json.Table.StartsWith("tradeBin") && json.Action == Action.Partial;
        }
        private void FireTradeBinEvents(Json json)
        {
            var tradeBin = json.Table.ToTradeBin();
            var emptyPartial = json.Data.Count == 0;
            OnTradeBinPartial(new TradeBinEventArgs(tradeBin, emptyPartial));
        }
        private bool IsTicker(Json json)
        {
            return json.Table == "trade" && json.Action != Action.Partial;
        }
        private void FireTickEvents(Json json)
        {
            foreach (var datum in json.Data)
            {
                OnTick(new TickEventArgs(datum.Symbol.ToEnum<Symbol>(), datum.Price));
            }
        }
        private void FireTableEvents(Json json)
        {
            var label = json.Table;
            var table = $"{Name}_{label}";
            switch (json.Action)
            {
                case Action.Partial:
                    OnTableCreated(new TableEventArgs(table, label));
                    break;
                case Action.Update:
                case Action.Insert:
                case Action.Delete:
                    OnTableChanged(new TableEventArgs(table, label));
                    break;
            }
        }
    }
}

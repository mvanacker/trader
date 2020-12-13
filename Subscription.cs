using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader
{
    public class Subscription
    {
        public Subscription() { }

        public Dictionary<string, HashSet<string>> Topics { get; } = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, int> Confirmations { get; } = new Dictionary<string, int>();

        public void AddTopic(string topic, string subtopic = null)
        {
            if (subtopic == null) subtopic = string.Empty;
            if (!Topics.ContainsKey(topic)) Topics[topic] = new HashSet<string>();
            Topics[topic].Add(subtopic);
            Confirmations[topic] = 0;
        }
        public bool IsConfirmed(string topic)
        {
            return Confirmations[topic] == Topics[topic].Count;
        }
        public bool IsConfirmed()
        {
            return Topics.Keys.All(topic => IsConfirmed(topic));
        }
        public override string ToString()
        {
            var args = string.Join(",", Topics.Keys.Select(topic =>
            {
                var subtopics = Topics[topic];
                if (subtopics.Count == 1 && string.IsNullOrEmpty(subtopics.First()))
                {
                    return $"\"{topic}\"";
                }
                else
                {
                    return string.Join(",", subtopics.Select(subtopic =>
                    {
                        return $"\"{topic}:{subtopic}\"";
                    }));
                }
            }));
            return $"{{\"op\":\"subscribe\",\"args\":[{args}]}}";
        }
    }
}

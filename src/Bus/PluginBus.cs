using System;
using System.Collections.Generic;
using Exiled.API.Features;

namespace AugatonLib.Bus
{
    public static class PluginBus
    {
        private sealed class Subscription
        {
            public Subscription(string owner, string topic, Action<BusMessage> handler)
            {
                Owner = owner;
                Topic = topic;
                Handler = handler;
            }

            public string Owner { get; }

            public string Topic { get; }

            public Action<BusMessage> Handler { get; }
        }

        private static readonly List<Subscription> Subscriptions = new List<Subscription>(16);
        private static readonly List<Subscription> Buffer = new List<Subscription>(8);

        public static int SubscriptionCount => Subscriptions.Count;

        public static void Subscribe(string owner, string topic, Action<BusMessage> handler)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(topic) || handler is null)
                return;

            Subscriptions.Add(new Subscription(owner, topic, handler));
        }

        public static void Unsubscribe(string owner, string topic)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(topic))
                return;

            Subscriptions.RemoveAll(subscription =>
                string.Equals(subscription.Owner, owner, StringComparison.Ordinal)
                && string.Equals(subscription.Topic, topic, StringComparison.OrdinalIgnoreCase));
        }

        public static void UnsubscribeOwner(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Subscriptions.RemoveAll(subscription => string.Equals(subscription.Owner, owner, StringComparison.Ordinal));
        }

        public static int Publish(string topic, string source, object payload = null)
        {
            if (string.IsNullOrEmpty(topic) || Subscriptions.Count == 0)
                return 0;

            Buffer.Clear();

            foreach (Subscription subscription in Subscriptions)
            {
                if (string.Equals(subscription.Topic, topic, StringComparison.OrdinalIgnoreCase))
                    Buffer.Add(subscription);
            }

            if (Buffer.Count == 0)
                return 0;

            BusMessage message = new BusMessage(topic, source, payload);
            int delivered = 0;

            foreach (Subscription subscription in Buffer)
            {
                try
                {
                    subscription.Handler(message);
                    delivered++;
                }
                catch (Exception e)
                {
                    Log.Error($"PluginBus: {subscription.Owner} sur \"{topic}\" : {e}");
                }
            }

            Buffer.Clear();
            return delivered;
        }

        public static void Clear()
        {
            Subscriptions.Clear();
            Buffer.Clear();
        }

        public static string Describe()
        {
            if (Subscriptions.Count == 0)
                return "aucun abonne";

            HashSet<string> topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> owners = new HashSet<string>(StringComparer.Ordinal);

            foreach (Subscription subscription in Subscriptions)
            {
                topics.Add(subscription.Topic);
                owners.Add(subscription.Owner);
            }

            return $"{Subscriptions.Count} abonnement(s), {topics.Count} sujet(s), {owners.Count} plugin(s)";
        }
    }
}

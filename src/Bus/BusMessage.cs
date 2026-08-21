namespace AugatonLib.Bus
{
    public sealed class BusMessage
    {
        public BusMessage(string topic, string source, object payload)
        {
            Topic = topic;
            Source = source;
            Payload = payload;
        }

        public string Topic { get; }

        public string Source { get; }

        public object Payload { get; }

        public bool TryGet<T>(out T value)
        {
            if (Payload is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        public string Text => Payload as string;
    }
}

namespace AugatonLib.Integrations
{
    public sealed class UncomplicatedEntry
    {
        public UncomplicatedEntry(uint id, string name)
        {
            Id = id;
            Name = name;
        }

        public uint Id { get; }

        public string Name { get; }

        public override string ToString() => $"{Id} - {Name}";
    }
}

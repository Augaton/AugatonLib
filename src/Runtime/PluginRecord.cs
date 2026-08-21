using System;

namespace AugatonLib.Runtime
{
    public sealed class PluginRecord
    {
        public PluginRecord(string name, string assemblyName, Version version, Capability[] capabilities)
        {
            Name = name;
            AssemblyName = assemblyName;
            Version = version;
            Capabilities = capabilities ?? Array.Empty<Capability>();
        }

        public string Name { get; }

        public string AssemblyName { get; }

        public Version Version { get; }

        public Capability[] Capabilities { get; }

        public bool Declares(Capability capability)
        {
            foreach (Capability entry in Capabilities)
            {
                if (entry == capability)
                    return true;
            }

            return false;
        }

        public string DescribeCapabilities()
        {
            if (Capabilities.Length == 0)
                return string.Empty;

            string[] labels = new string[Capabilities.Length];

            for (int i = 0; i < Capabilities.Length; i++)
                labels[i] = CapabilityLabel.Short(Capabilities[i]);

            return string.Join(" ", labels);
        }
    }
}

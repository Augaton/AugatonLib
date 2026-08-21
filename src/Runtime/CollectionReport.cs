using System;
using System.Text;
using AugatonLib.Arbitration;
using AugatonLib.Bus;
using AugatonLib.FriendlyFire;
using AugatonLib.Hints;

namespace AugatonLib.Runtime
{
    public static class CollectionReport
    {
        public static string Text()
        {
            StringBuilder builder = new StringBuilder(1024);

            builder.AppendLine($"AugatonLib {Version(PluginDirectory.LibraryVersion)} - collection Zone-Shilari");
            builder.AppendLine();

            AppendCollection(builder);
            AppendIntegrations(builder);
            AppendArbiters(builder);

            return builder.ToString();
        }

        private static void AppendCollection(StringBuilder builder)
        {
            int active = 0;

            foreach (KnownPlugin known in KnownPlugins.All)
            {
                if (PluginDirectory.IsRegistered(known.AssemblyName))
                    active++;
            }

            builder.AppendLine($"Plugins ({active}/{KnownPlugins.All.Count} enregistres) :");

            foreach (KnownPlugin known in KnownPlugins.All)
            {
                PluginRecord record = PluginDirectory.Find(known.AssemblyName);

                if (record is not null)
                {
                    string capabilities = record.DescribeCapabilities();

                    builder.AppendLine($"  [+] {known.DisplayName} {Version(record.Version)}" +
                                       (string.IsNullOrEmpty(capabilities) ? string.Empty : $" - {capabilities}"));
                    continue;
                }

                if (IntegrationProbe.IsLoaded(known.AssemblyName))
                {
                    builder.AppendLine($"  [~] {known.DisplayName} - charge, non enregistre");
                    continue;
                }

                builder.AppendLine($"  [-] {known.DisplayName} - absent");
            }

            builder.AppendLine();
        }

        private static void AppendIntegrations(StringBuilder builder)
        {
            builder.AppendLine("Integrations :");

            foreach (Integration integration in IntegrationProbe.All)
            {
                Version version = IntegrationProbe.VersionOf(integration.AssemblyName);

                builder.AppendLine(version is null
                    ? $"  [-] {integration.DisplayName} - absent"
                    : $"  [+] {integration.DisplayName} {Version(version)}");
            }

            builder.AppendLine();
        }

        private static void AppendArbiters(StringBuilder builder)
        {
            builder.AppendLine("Arbitres :");
            builder.AppendLine($"  tir allie : {FriendlyFireArbiter.Describe()}");
            builder.AppendLine($"  echelle   : {ScaleArbiter.Describe()}");
            builder.AppendLine($"  lumieres  : {LightArbiter.Describe()}");
            builder.AppendLine($"  portes    : {DoorLockRegistry.Describe()}");
            builder.AppendLine($"  departs   : {DepartureArbiter.Describe()}");
            builder.AppendLine($"  genocide  : {GenocideArbiter.Describe()}");
            builder.AppendLine($"  bus       : {PluginBus.Describe()}");
            builder.AppendLine($"  hints     : {(HintChannel.ServiceAvailable ? "HintServiceMeow" : "natifs (repli)")}");
        }

        private static string Version(Version version)
        {
            return version is null ? "--" : $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}

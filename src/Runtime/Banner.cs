using System;
using System.Collections.Generic;
using System.Text;
using AugatonLib.Arbitration;
using AugatonLib.Bus;
using AugatonLib.FriendlyFire;
using AugatonLib.Hints;
using Exiled.API.Features;

namespace AugatonLib.Runtime
{
    public static class Banner
    {
        private sealed class Pairing
        {
            public Pairing(Capability capability, string left, string[] partners, string consequence)
            {
                Capability = capability;
                Left = left;
                Partners = partners;
                Consequence = consequence;
            }

            public Capability Capability { get; }

            public string Left { get; }

            public string[] Partners { get; }

            public string Consequence { get; }
        }

        private const int Width = 78;
        private const int NameColumn = 26;
        private const int VersionColumn = 8;
        private const int RoleColumn = 22;

        private static readonly Pairing[] Pairings =
        {
            new Pairing(
                Capability.Scale,
                "RealisticSizes",
                new[] { "SCP500s", "BetterCoinflipsRewritten", "RandomGamemode" },
                "les tailles RP seront ecrasees par les effets"),
            new Pairing(
                Capability.DoorLock,
                "RemoteKeycard",
                new[] { "RandomGamemode" },
                "les confinements seront contournes par la carte a distance"),
            new Pairing(
                Capability.Departure,
                "Replacer",
                new[] { "SCPReplacer" },
                "un SCP qui part sera traite par les deux plugins"),
            new Pairing(
                Capability.Genocide,
                "TeamGenocide",
                new[] { "RandomGamemode" },
                "les conversions de mode declencheront de fausses annonces"),
            new Pairing(
                Capability.Light,
                "BetterCoinflipsRewritten",
                new[] { "RandomGamemode", "SCP500s", "TeamGenocide" },
                "les effets de lumiere s'annuleront entre eux"),
        };

        public static void Print()
        {
            try
            {
                Header();
                Collection();
                Integrations();
                Arbiters();
                Warnings();
                Blank();
            }
            catch (Exception e)
            {
                Log.Error($"Banner: {e}");
            }
        }

        private static void Header()
        {
            Blank();
            Write(Frame(), ConsoleColor.DarkCyan);
            Write(Box("  A U G A T O N L I B", $"v{Text(PluginDirectory.LibraryVersion)}  "), ConsoleColor.Cyan);
            Write(Box($"  Zone-Shilari  |  EXILED {Text(ExiledVersion)}  |  net48", string.Empty), ConsoleColor.DarkGray);
            Write(Frame(), ConsoleColor.DarkCyan);
            Blank();
        }

        private static void Collection()
        {
            int active = 0;
            int stale = 0;

            foreach (KnownPlugin known in KnownPlugins.All)
            {
                if (PluginDirectory.IsRegistered(known.AssemblyName))
                    active++;
                else if (IntegrationProbe.IsLoaded(known.AssemblyName))
                    stale++;
            }

            Section("COLLECTION", $"{active}/{KnownPlugins.All.Count} enregistres");

            foreach (KnownPlugin known in KnownPlugins.All)
            {
                PluginRecord record = PluginDirectory.Find(known.AssemblyName);

                if (record is not null)
                {
                    Write(Row("[+]", known.DisplayName, Text(record.Version), known.Role, record.DescribeCapabilities()), ConsoleColor.Green);
                    continue;
                }

                Version loaded = IntegrationProbe.VersionOf(known.AssemblyName);

                if (loaded is not null)
                {
                    Write(Row("[~]", known.DisplayName, Text(loaded), known.Role, "non enregistre"), ConsoleColor.Yellow);
                    continue;
                }

                Write(Row("[-]", known.DisplayName, "--", known.Role, "absent"), ConsoleColor.DarkGray);
            }

            if (stale > 0)
                Write(Rule(), ConsoleColor.DarkGray);

            Blank();
        }

        private static void Integrations()
        {
            Section("INTEGRATIONS", string.Empty);

            foreach (Integration integration in IntegrationProbe.All)
            {
                Version version = IntegrationProbe.VersionOf(integration);

                if (version is not null)
                {
                    Write(Row("[+]", integration.DisplayName, Text(version), integration.Role, string.Empty), ConsoleColor.Green);
                    continue;
                }

                string note = integration.Required ? "recommande" : "optionnel";

                Write(Row("[-]", integration.DisplayName, "--", integration.Role, note), ConsoleColor.DarkGray);
            }

            Blank();
        }

        private static void Arbiters()
        {
            Section("ARBITRES", string.Empty);

            Write(Detail("tir allie", FriendlyFireArbiter.Describe()), ConsoleColor.Gray);
            Write(Detail("echelle", ScaleArbiter.Describe()), ConsoleColor.Gray);
            Write(Detail("lumieres", LightArbiter.Describe()), ConsoleColor.Gray);
            Write(Detail("portes", DoorLockRegistry.Describe()), ConsoleColor.Gray);
            Write(Detail("departs", DepartureArbiter.Describe()), ConsoleColor.Gray);
            Write(Detail("genocide", GenocideArbiter.Describe()), ConsoleColor.Gray);
            Write(Detail("bus", PluginBus.Describe()), ConsoleColor.Gray);
            Write(Detail("hints", HintStatus()), ConsoleColor.Gray);

            Blank();
        }

        private static void Warnings()
        {
            List<string> warnings = new List<string>(8);

            foreach (KnownPlugin known in KnownPlugins.All)
            {
                if (PluginDirectory.IsRegistered(known.AssemblyName) || !IntegrationProbe.IsLoaded(known.AssemblyName))
                    continue;

                warnings.Add($"{known.DisplayName} est charge mais ne s'enregistre pas : version anterieure a l'arbitrage.");
            }

            foreach (Pairing pairing in Pairings)
            {
                if (!PluginDirectory.IsLoaded(pairing.Left))
                    continue;

                foreach (string partner in pairing.Partners)
                {
                    if (!PluginDirectory.IsLoaded(partner))
                        continue;

                    if (PluginDirectory.Declares(pairing.Left, pairing.Capability)
                        && PluginDirectory.Declares(partner, pairing.Capability))
                    {
                        continue;
                    }

                    warnings.Add($"{pairing.Left} et {partner} n'arbitrent pas \"{CapabilityLabel.Short(pairing.Capability)}\" : {pairing.Consequence}.");
                }
            }

            if (!IntegrationProbe.IsPresent("HintServiceMeow") && HasCapability(Capability.Hints))
                warnings.Add("HintServiceMeow absent : les hints se remplaceront mutuellement.");

            if (warnings.Count == 0)
            {
                Write("   [ok] Aucun conflit detecte entre les plugins charges.", ConsoleColor.DarkGreen);
                return;
            }

            foreach (string warning in warnings)
                Write($"   [!] {warning}", ConsoleColor.Yellow);
        }

        private static string HintStatus()
        {
            if (!IntegrationProbe.IsPresent("HintServiceMeow"))
                return "natifs (HintServiceMeow absent)";

            return HintChannel.ServiceAvailable ? "HintServiceMeow" : "natifs (repli apres echec)";
        }

        private static bool HasCapability(Capability capability)
        {
            foreach (PluginRecord record in PluginDirectory.All)
            {
                if (record.Declares(capability))
                    return true;
            }

            return false;
        }

        private static Version ExiledVersion => typeof(Log).Assembly.GetName().Version;

        private static void Section(string title, string right)
        {
            StringBuilder builder = new StringBuilder(Width + 4);

            builder.Append("  ").Append(title);

            if (!string.IsNullOrEmpty(right))
            {
                int padding = Width - title.Length - right.Length;

                builder.Append(padding > 0 ? new string(' ', padding) : " ").Append(right);
            }

            Write(builder.ToString(), ConsoleColor.White);
            Write(Rule(), ConsoleColor.DarkGray);
        }

        private static string Row(string marker, string name, string version, string role, string extra)
        {
            StringBuilder builder = new StringBuilder(Width + 8);

            builder.Append("   ").Append(marker).Append(' ');
            builder.Append(Pad(name, NameColumn));
            builder.Append(Pad(version, VersionColumn));
            builder.Append(Pad(role, RoleColumn));
            builder.Append(extra);

            return builder.ToString().TrimEnd();
        }

        private static string Detail(string label, string value)
        {
            return $"   {Pad(label, 12)}{value}";
        }

        private static string Box(string left, string right)
        {
            int padding = Width - left.Length - right.Length;

            if (padding < 1)
                padding = 1;

            return $"  |{left}{new string(' ', padding)}{right}|";
        }

        private static string Frame() => "  +" + new string('-', Width) + "+";

        private static string Rule() => "  " + new string('-', Width);

        private static string Pad(string value, int size)
        {
            if (value is null)
                value = string.Empty;

            return value.Length >= size ? value.Substring(0, size - 1) + " " : value.PadRight(size);
        }

        private static string Text(Version version)
        {
            return version is null ? "--" : $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private static void Blank() => Write(string.Empty, ConsoleColor.Gray);

        private static void Write(string line, ConsoleColor color) => Log.SendRaw(line, color);
    }
}

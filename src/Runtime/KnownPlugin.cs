using System.Collections.Generic;

namespace AugatonLib.Runtime
{
    public sealed class KnownPlugin
    {
        public KnownPlugin(string assemblyName, string displayName, string role)
        {
            AssemblyName = assemblyName;
            DisplayName = displayName;
            Role = role;
        }

        public string AssemblyName { get; }

        public string DisplayName { get; }

        public string Role { get; }
    }

    public static class KnownPlugins
    {
        private static readonly KnownPlugin[] Catalog =
        {
            new KnownPlugin("AutoFFToggle", "AutoFFToggle", "tir allie auto"),
            new KnownPlugin("BetterCoinflipsRewritten", "BetterCoinflips", "pile ou face"),
            new KnownPlugin("RandomGamemode", "RandomGamemode", "modes de jeu"),
            new KnownPlugin("RealisticSizes", "RealisticSizes", "taille par joueur"),
            new KnownPlugin("RemoteKeycard", "RemoteKeycard", "ouverture a distance"),
            new KnownPlugin("Replacer", "Replacer", "remplacement joueur"),
            new KnownPlugin("SCP1162", "SCP1162", "echange d'objets"),
            new KnownPlugin("SCP500s", "SCP500s", "pilules SCP-500"),
            new KnownPlugin("ScpProximityChat", "ScpProximityChat", "proximite SCP"),
            new KnownPlugin("SCPReplacer", "SCPReplacer", "volontariat SCP"),
            new KnownPlugin("SupplyDrop", "SupplyDrop", "largages"),
            new KnownPlugin("TeamGenocide", "TeamGenocide", "extinction d'equipe"),
        };

        public static IReadOnlyList<KnownPlugin> All => Catalog;

        public static KnownPlugin Find(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return null;

            foreach (KnownPlugin entry in Catalog)
            {
                if (string.Equals(entry.AssemblyName, assemblyName, System.StringComparison.OrdinalIgnoreCase))
                    return entry;
            }

            return null;
        }
    }
}

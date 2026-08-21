using System;
using System.Collections.Generic;
using System.Reflection;

namespace AugatonLib.Runtime
{
    public sealed class Integration
    {
        public Integration(string assemblyName, string displayName, string role, bool required)
        {
            AssemblyName = assemblyName;
            DisplayName = displayName;
            Role = role;
            Required = required;
        }

        public string AssemblyName { get; }

        public string DisplayName { get; }

        public string Role { get; }

        public bool Required { get; }
    }

    public static class IntegrationProbe
    {
        private static readonly Integration[] Catalog =
        {
            new Integration("HintServiceMeow", "HintServiceMeow", "hints coordonnes", true),
            new Integration("Exiled.CustomItems", "Exiled.CustomItems", "objets personnalises", false),
            new Integration("Exiled.CustomRoles", "Exiled.CustomRoles", "roles personnalises", false),
            new Integration("UncomplicatedCustomItems", "UncomplicatedCustomItems", "objets UC", false),
            new Integration("UncomplicatedCustomRoles", "UncomplicatedCustomRoles", "roles UC", false),
            new Integration("UncomplicatedCustomTeams", "UncomplicatedCustomTeams", "equipes UC", false),
        };

        public static IReadOnlyList<Integration> All => Catalog;

        public static bool IsLoaded(string assemblyName) => VersionOf(assemblyName) is not null;

        public static Version VersionOf(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName name = SafeName(assembly);

                if (name is null)
                    continue;

                if (name.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                    return name.Version;
            }

            return null;
        }

        private static AssemblyName SafeName(Assembly assembly)
        {
            try
            {
                return assembly?.GetName();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

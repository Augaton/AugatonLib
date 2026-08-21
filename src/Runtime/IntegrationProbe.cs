using System;
using System.Collections.Generic;
using System.Reflection;

namespace AugatonLib.Runtime
{
    public sealed class Integration
    {
        public Integration(string[] assemblyNames, string displayName, string role, bool required)
        {
            AssemblyNames = assemblyNames;
            DisplayName = displayName;
            Role = role;
            Required = required;
        }

        public string[] AssemblyNames { get; }

        public string DisplayName { get; }

        public string Role { get; }

        public bool Required { get; }
    }

    public static class IntegrationProbe
    {
        private static readonly Integration[] Catalog =
        {
            new Integration(
                new[] { "HintServiceMeow-Exiled", "HintServiceMeow" },
                "HintServiceMeow",
                "hints coordonnes",
                true),
            new Integration(
                new[] { "Exiled.CustomItems" },
                "Exiled.CustomItems",
                "objets personnalises",
                false),
            new Integration(
                new[] { "Exiled.CustomRoles" },
                "Exiled.CustomRoles",
                "roles personnalises",
                false),
            new Integration(
                new[] { "UncomplicatedCustomItems-Exiled", "UncomplicatedCustomItems" },
                "UncomplicatedCustomItems",
                "objets UC",
                false),
            new Integration(
                new[] { "UncomplicatedCustomRoles" },
                "UncomplicatedCustomRoles",
                "roles UC",
                false),
            new Integration(
                new[] { "UncomplicatedCustomTeams" },
                "UncomplicatedCustomTeams",
                "equipes UC",
                false),
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

                if (name is null || !name.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                    continue;

                return Declared(assembly) ?? name.Version;
            }

            return null;
        }

        public static Version VersionOf(Integration integration)
        {
            if (integration is null)
                return null;

            foreach (string candidate in integration.AssemblyNames)
            {
                Version found = VersionOf(candidate);

                if (found is not null)
                    return found;
            }

            return null;
        }

        public static bool IsLoaded(Integration integration) => VersionOf(integration) is not null;

        public static Integration Find(string displayName)
        {
            foreach (Integration integration in Catalog)
            {
                if (integration.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    return integration;
            }

            return null;
        }

        public static bool IsPresent(string displayName) => IsLoaded(Find(displayName));

        private static Version Declared(Assembly assembly)
        {
            try
            {
                string raw = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

                if (string.IsNullOrEmpty(raw))
                    return null;

                int cut = raw.IndexOfAny(new[] { '-', '+', ' ' });

                if (cut > 0)
                    raw = raw.Substring(0, cut);

                return Version.TryParse(raw, out Version parsed) ? parsed : null;
            }
            catch (Exception)
            {
                return null;
            }
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

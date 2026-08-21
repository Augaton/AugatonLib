using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;

namespace AugatonLib.Integrations
{
    public sealed class UncomplicatedBridge
    {
        private const string RolesAssembly = "UncomplicatedCustomRoles";
        private const string ItemsAssembly = "UncomplicatedCustomItems-Exiled";
        private const string TeamsAssembly = "UncomplicatedCustomTeams";

        private readonly List<UncomplicatedEntry> roles = new List<UncomplicatedEntry>();
        private readonly List<UncomplicatedEntry> items = new List<UncomplicatedEntry>();
        private readonly List<UncomplicatedEntry> teams = new List<UncomplicatedEntry>();

        private PropertyInfo roleList;
        private MethodInfo roleSummon;
        private MethodInfo labPlayerGet;

        private PropertyInfo itemList;
        private ConstructorInfo itemSummon;

        private PropertyInfo teamList;
        private MethodInfo teamSpawn;

        public IReadOnlyList<UncomplicatedEntry> Roles => roles;

        public IReadOnlyList<UncomplicatedEntry> Items => items;

        public IReadOnlyList<UncomplicatedEntry> Teams => teams;

        public bool RolesAvailable => roleSummon is not null && roleList is not null && labPlayerGet is not null;

        public bool ItemsAvailable => itemSummon is not null && itemList is not null;

        public bool TeamsAvailable => teamSpawn is not null && teamList is not null;

        public string LastError { get; private set; }

        public void Initialise()
        {
            roles.Clear();
            items.Clear();
            teams.Clear();

            ResolveRoles();
            ResolveItems();
            ResolveTeams();

            Refresh();

            Log.Info(
                $"[AugatonLib] Uncomplicated - roles {Status(RolesAvailable, roles.Count)}, " +
                $"objets {Status(ItemsAvailable, items.Count)}, " +
                $"equipes {Status(TeamsAvailable, teams.Count)}.");
        }

        public void Refresh()
        {
            if (!RolesAvailable)
                ResolveRoles();

            if (!ItemsAvailable)
                ResolveItems();

            if (!TeamsAvailable)
                ResolveTeams();

            Fill(roles, roleList, "Id", "Name");
            Fill(items, itemList, "Id", "Name");
            Fill(teams, teamList, "Id", "Name");
        }

        public UncomplicatedEntry RandomRole() => Random.Chance.Pick(roles);

        public UncomplicatedEntry RandomItem() => Random.Chance.Pick(items);

        public UncomplicatedEntry RandomTeam() => Random.Chance.Pick(teams);

        public bool TryApplyRole(Player player, uint id)
        {
            if (player?.ReferenceHub is null || !RolesAvailable)
                return false;

            try
            {
                object definition = FindDefinition(roleList, id);

                if (definition is null)
                    return false;

                object labPlayer = labPlayerGet.Invoke(null, new object[] { player.ReferenceHub });

                if (labPlayer is null)
                    return false;

                roleSummon.Invoke(null, new[] { labPlayer, definition });
                return true;
            }
            catch (Exception e)
            {
                LastError = Unwrap(e);
                Log.Warn($"[AugatonLib] Role personnalise {id} non applique : {LastError}");
                return false;
            }
        }

        public bool TryGiveItem(Player player, uint id)
        {
            if (player is null || !ItemsAvailable)
                return false;

            try
            {
                object definition = FindDefinition(itemList, id);

                if (definition is null)
                    return false;

                itemSummon.Invoke(new object[] { definition, player });
                return true;
            }
            catch (Exception e)
            {
                LastError = Unwrap(e);
                Log.Warn($"[AugatonLib] Objet personnalise {id} non donne : {LastError}");
                return false;
            }
        }

        public bool TrySpawnTeam(uint id)
        {
            if (!TeamsAvailable)
                return false;

            try
            {
                object definition = FindDefinition(teamList, id);

                if (definition is null)
                    return false;

                object[] arguments = BuildSpawnArguments(teamSpawn, definition);
                teamSpawn.Invoke(null, arguments);
                return true;
            }
            catch (Exception e)
            {
                LastError = Unwrap(e);
                Log.Warn($"[AugatonLib] Equipe personnalisee {id} non invoquee : {LastError}");
                return false;
            }
        }

        private void ResolveRoles()
        {
            Type customRole = FindType(RolesAssembly, "UncomplicatedCustomRoles.API.Features.CustomRole");
            Type summoned = FindType(RolesAssembly, "UncomplicatedCustomRoles.API.Features.SummonedCustomRole");
            Type labPlayer = FindType("LabApi", "LabApi.Features.Wrappers.Player")
                             ?? FindTypeAnywhere("LabApi.Features.Wrappers.Player");

            roleList = customRole?.GetProperty("List", BindingFlags.Public | BindingFlags.Static);
            roleSummon = FindStatic(summoned, "Summon", 2);
            labPlayerGet = labPlayer?.GetMethod("Get", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ReferenceHub) }, null);
        }

        private void ResolveItems()
        {
            Type customItem = FindType(ItemsAssembly, "UncomplicatedCustomItems.API.Features.CustomItem");
            Type summoned = FindType(ItemsAssembly, "UncomplicatedCustomItems.API.Features.SummonedCustomItem");

            itemList = customItem?.GetProperty("List", BindingFlags.Public | BindingFlags.Static);

            if (summoned is null)
                return;

            foreach (ConstructorInfo constructor in summoned.GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Player))
                {
                    itemSummon = constructor;
                    return;
                }
            }
        }

        private void ResolveTeams()
        {
            Type team = FindType(TeamsAssembly, "UncomplicatedCustomTeams.API.Features.Definitions.Team");
            Type spawner = FindType(TeamsAssembly, "UncomplicatedCustomTeams.API.Features.Services.TeamSpawner");

            teamList = team?.GetProperty("List", BindingFlags.Public | BindingFlags.Static);
            teamSpawn = FindStatic(spawner, "SpawnSpecificTeam", -1);
        }

        private static object[] BuildSpawnArguments(MethodInfo method, object definition)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments = new object[parameters.Length];
            arguments[0] = definition;

            for (int i = 1; i < parameters.Length; i++)
            {
                arguments[i] = parameters[i].HasDefaultValue
                    ? parameters[i].DefaultValue
                    : parameters[i].ParameterType == typeof(bool) ? (object)false : null;
            }

            return arguments;
        }

        private static object FindDefinition(PropertyInfo listProperty, uint id)
        {
            if (listProperty?.GetValue(null) is not IEnumerable enumerable)
                return null;

            foreach (object candidate in enumerable)
            {
                if (candidate is null)
                    continue;

                if (ReadId(candidate) == id)
                    return candidate;
            }

            return null;
        }

        private static void Fill(List<UncomplicatedEntry> target, PropertyInfo listProperty, string idName, string nameName)
        {
            target.Clear();

            if (listProperty?.GetValue(null) is not IEnumerable enumerable)
                return;

            foreach (object candidate in enumerable)
            {
                if (candidate is null)
                    continue;

                uint id = ReadId(candidate);
                object name = candidate.GetType().GetProperty(nameName)?.GetValue(candidate);

                target.Add(new UncomplicatedEntry(id, name as string ?? $"#{id}"));
            }
        }

        private static uint ReadId(object instance)
        {
            object raw = instance.GetType().GetProperty("Id")?.GetValue(instance);

            return raw switch
            {
                uint value => value,
                int value when value >= 0 => (uint)value,
                _ => 0u,
            };
        }

        private static MethodInfo FindStatic(Type type, string name, int parameterCount)
        {
            if (type is null)
                return null;

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!method.Name.Equals(name, StringComparison.Ordinal))
                    continue;

                if (parameterCount < 0 || method.GetParameters().Length == parameterCount)
                    return method;
            }

            return null;
        }

        private static Type FindType(string assemblyName, string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                    continue;

                Type found = SafeGetType(assembly, fullName);

                if (found is not null)
                    return found;
            }

            return null;
        }

        private static Type FindTypeAnywhere(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found = SafeGetType(assembly, fullName);

                if (found is not null)
                    return found;
            }

            return null;
        }

        private static Type SafeGetType(Assembly assembly, string fullName)
        {
            try
            {
                return assembly.GetType(fullName, false);
            }
            catch
            {
                return null;
            }
        }

        private static string Status(bool available, int count)
        {
            return available ? $"OK ({count})" : "indisponible";
        }

        private static string Unwrap(Exception e)
        {
            return e.InnerException?.Message ?? e.Message;
        }
    }
}

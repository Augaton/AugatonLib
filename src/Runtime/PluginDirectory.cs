using System;
using System.Collections.Generic;
using System.Reflection;
using Exiled.API.Interfaces;
using MEC;

namespace AugatonLib.Runtime
{
    public static class PluginDirectory
    {
        private static readonly Dictionary<string, PluginRecord> Entries =
            new Dictionary<string, PluginRecord>(StringComparer.OrdinalIgnoreCase);

        private static CoroutineHandle bannerHandle;
        private static bool bannerScheduled;

        public static float BannerDelay { get; set; } = 3f;

        public static int Count => Entries.Count;

        public static IEnumerable<PluginRecord> All => Entries.Values;

        public static Version LibraryVersion => typeof(PluginDirectory).Assembly.GetName().Version;

        public static void Register(IPlugin<IConfig> plugin, params Capability[] capabilities)
        {
            if (plugin is null)
                return;

            string assemblyName = ResolveAssemblyName(plugin);

            if (string.IsNullOrEmpty(assemblyName))
                return;

            Entries[assemblyName] = new PluginRecord(plugin.Name, assemblyName, plugin.Version, capabilities);

            RoundReset.Hook();
            ScheduleBanner();
        }

        public static void Unregister(IPlugin<IConfig> plugin)
        {
            if (plugin is null)
                return;

            string assemblyName = ResolveAssemblyName(plugin);

            if (!string.IsNullOrEmpty(assemblyName))
                Entries.Remove(assemblyName);
        }

        public static PluginRecord Find(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return null;

            return Entries.TryGetValue(assemblyName, out PluginRecord record) ? record : null;
        }

        public static bool IsRegistered(string assemblyName) => Find(assemblyName) is not null;

        public static bool Declares(string assemblyName, Capability capability)
        {
            PluginRecord record = Find(assemblyName);

            return record is not null && record.Declares(capability);
        }

        public static Version LoadedVersion(string assemblyName)
        {
            PluginRecord record = Find(assemblyName);

            return record is not null ? record.Version : IntegrationProbe.VersionOf(assemblyName);
        }

        public static bool IsLoaded(string assemblyName)
        {
            return IsRegistered(assemblyName) || IntegrationProbe.IsLoaded(assemblyName);
        }

        public static void PrintBanner()
        {
            CancelBanner();
            Banner.Print();
        }

        private static void ScheduleBanner()
        {
            try
            {
                CancelBanner();

                bannerScheduled = true;
                bannerHandle = Timing.CallDelayed(BannerDelay, () =>
                {
                    bannerScheduled = false;
                    Banner.Print();
                });
            }
            catch (Exception e)
            {
                bannerScheduled = false;
                Exiled.API.Features.Log.Debug($"PluginDirectory: banniere non planifiee ({e.Message}).");
            }
        }

        private static void CancelBanner()
        {
            if (!bannerScheduled)
                return;

            Timing.KillCoroutines(bannerHandle);
            bannerScheduled = false;
        }

        private static string ResolveAssemblyName(IPlugin<IConfig> plugin)
        {
            Assembly assembly = plugin.Assembly;

            if (assembly is null)
                return plugin.Name;

            try
            {
                return assembly.GetName().Name;
            }
            catch (Exception)
            {
                return plugin.Name;
            }
        }
    }
}

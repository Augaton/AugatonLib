using System;
using System.Collections.Generic;
using Exiled.API.Features;

namespace AugatonLib.FriendlyFire
{
    public static class FriendlyFireArbiter
    {
        private const string OnKey = "AugatonLib.FriendlyFire.RequestOn";
        private const string OffKey = "AugatonLib.FriendlyFire.RequestOff";
        private const string BaselineKey = "AugatonLib.FriendlyFire.Baseline";

        public static void RequestOn(string owner) => Request(OnKey, owner);

        public static void RequestOff(string owner) => Request(OffKey, owner);

        public static void Release(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            bool removed = Holders(OnKey).Remove(owner) | Holders(OffKey).Remove(owner);

            if (!removed)
                return;

            Resolve();
        }

        public static bool IsHeldBy(string owner)
        {
            return !string.IsNullOrEmpty(owner)
                && (Holders(OnKey).Contains(owner) || Holders(OffKey).Contains(owner));
        }

        public static string Describe()
        {
            HashSet<string> on = Holders(OnKey);
            HashSet<string> off = Holders(OffKey);

            string onList = on.Count == 0 ? "aucun" : string.Join(", ", new List<string>(on).ToArray());
            string offList = off.Count == 0 ? "aucun" : string.Join(", ", new List<string>(off).ToArray());

            return $"tir allie {(Server.FriendlyFire ? "actif" : "inactif")}, " +
                   $"base {Baseline}, demandes ON [{onList}], demandes OFF [{offList}]";
        }

        private static void Request(string key, string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            EnsureBaseline();

            Holders(key == OnKey ? OffKey : OnKey).Remove(owner);
            Holders(key).Add(owner);

            Resolve();
        }

        private static void Resolve()
        {
            bool target;

            if (Holders(OnKey).Count > 0)
                target = true;
            else if (Holders(OffKey).Count > 0)
                target = false;
            else
                target = Baseline;

            if (Server.FriendlyFire != target)
                Server.FriendlyFire = target;
        }

        private static void EnsureBaseline()
        {
            if (Holders(OnKey).Count > 0 || Holders(OffKey).Count > 0)
                return;

            AppDomain.CurrentDomain.SetData(BaselineKey, Server.FriendlyFire);
        }

        private static bool Baseline
        {
            get
            {
                object stored = AppDomain.CurrentDomain.GetData(BaselineKey);

                return stored is bool value ? value : Server.FriendlyFire;
            }
        }

        private static HashSet<string> Holders(string key)
        {
            if (AppDomain.CurrentDomain.GetData(key) is HashSet<string> existing)
                return existing;

            HashSet<string> created = new HashSet<string>();
            AppDomain.CurrentDomain.SetData(key, created);
            return created;
        }
    }
}

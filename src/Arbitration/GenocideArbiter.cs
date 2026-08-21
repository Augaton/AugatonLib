using System;
using System.Collections.Generic;
using UnityEngine;

namespace AugatonLib.Arbitration
{
    public static class GenocideArbiter
    {
        private const float Permanent = float.MaxValue;

        private static readonly Dictionary<string, float> Suppressors =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public static bool IsSuppressed
        {
            get
            {
                Purge();
                return Suppressors.Count > 0;
            }
        }

        public static int Count
        {
            get
            {
                Purge();
                return Suppressors.Count;
            }
        }

        public static void Suppress(string owner) => Suppress(owner, 0f);

        public static void Suppress(string owner, float seconds)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Suppressors[owner] = seconds <= 0f
                ? Permanent
                : Time.realtimeSinceStartup + seconds;
        }

        public static void Allow(string owner)
        {
            if (!string.IsNullOrEmpty(owner))
                Suppressors.Remove(owner);
        }

        public static void Clear() => Suppressors.Clear();

        public static string Describe()
        {
            Purge();

            if (Suppressors.Count == 0)
                return "libre";

            string[] owners = new string[Suppressors.Count];
            Suppressors.Keys.CopyTo(owners, 0);

            return $"supprime par {string.Join(", ", owners)}";
        }

        private static void Purge()
        {
            if (Suppressors.Count == 0)
                return;

            float now = Time.realtimeSinceStartup;
            List<string> expired = null;

            foreach (KeyValuePair<string, float> entry in Suppressors)
            {
                if (entry.Value > now)
                    continue;

                expired ??= new List<string>(2);
                expired.Add(entry.Key);
            }

            if (expired is null)
                return;

            foreach (string owner in expired)
                Suppressors.Remove(owner);
        }
    }
}

using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using UnityEngine;

namespace AugatonLib.Arbitration
{
    public static class LightArbiter
    {
        private sealed class BlackoutClaim
        {
            public BlackoutClaim(string owner, ZoneType[] zones, float expiry)
            {
                Owner = owner;
                Zones = zones;
                Expiry = expiry;
            }

            public string Owner { get; }

            public ZoneType[] Zones { get; }

            public float Expiry { get; }
        }

        private sealed class TintClaim
        {
            public TintClaim(string owner, Color color)
            {
                Owner = owner;
                Color = color;
            }

            public string Owner { get; }

            public Color Color { get; }
        }

        public static readonly ZoneType[] AllZones =
        {
            ZoneType.Surface,
            ZoneType.Entrance,
            ZoneType.HeavyContainment,
            ZoneType.LightContainment,
        };

        private static readonly List<BlackoutClaim> Blackouts = new List<BlackoutClaim>(4);
        private static readonly List<TintClaim> Tints = new List<TintClaim>(4);

        public static int BlackoutCount
        {
            get
            {
                Purge();
                return Blackouts.Count;
            }
        }

        public static int TintCount => Tints.Count;

        public static void Blackout(string owner, float duration, ZoneType[] zones = null)
        {
            if (string.IsNullOrEmpty(owner) || duration <= 0f)
                return;

            Purge();

            ZoneType[] target = zones is null || zones.Length == 0 ? AllZones : zones;

            Blackouts.RemoveAll(claim => string.Equals(claim.Owner, owner, StringComparison.Ordinal));
            Blackouts.Add(new BlackoutClaim(owner, target, Time.realtimeSinceStartup + duration));

            Map.TurnOffAllLights(duration, target);
        }

        public static void Restore(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Purge();

            ZoneType[] released = null;

            foreach (BlackoutClaim claim in Blackouts)
            {
                if (!string.Equals(claim.Owner, owner, StringComparison.Ordinal))
                    continue;

                released = claim.Zones;
                break;
            }

            if (released is null)
                return;

            Blackouts.RemoveAll(claim => string.Equals(claim.Owner, owner, StringComparison.Ordinal));

            List<ZoneType> free = new List<ZoneType>(released.Length);

            foreach (ZoneType zone in released)
            {
                if (IsClaimed(zone))
                    continue;

                free.Add(zone);
            }

            if (free.Count > 0)
                Map.TurnOnAllLights(free);
        }

        public static bool IsBlackedOut(ZoneType zone)
        {
            Purge();
            return IsClaimed(zone);
        }

        public static void Tint(string owner, Color color)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Tints.RemoveAll(claim => string.Equals(claim.Owner, owner, StringComparison.Ordinal));
            Tints.Add(new TintClaim(owner, color));

            Map.ChangeLightsColor(color);
        }

        public static void ReleaseTint(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            if (Tints.RemoveAll(claim => string.Equals(claim.Owner, owner, StringComparison.Ordinal)) == 0)
                return;

            if (Tints.Count == 0)
            {
                Map.ResetLightsColor();
                return;
            }

            Map.ChangeLightsColor(Tints[Tints.Count - 1].Color);
        }

        public static void ReleaseOwner(string owner)
        {
            Restore(owner);
            ReleaseTint(owner);
        }

        public static void Clear()
        {
            Blackouts.Clear();

            if (Tints.Count > 0)
            {
                Tints.Clear();
                Map.ResetLightsColor();
            }
        }

        public static string Describe()
        {
            Purge();

            if (Blackouts.Count == 0 && Tints.Count == 0)
                return "libre";

            string blackout = Blackouts.Count == 0 ? "aucune" : Join(Blackouts);
            string tint = Tints.Count == 0 ? "aucune" : Tints[Tints.Count - 1].Owner;

            return $"pannes [{blackout}], teinte [{tint}]";
        }

        private static bool IsClaimed(ZoneType zone)
        {
            foreach (BlackoutClaim claim in Blackouts)
            {
                foreach (ZoneType claimed in claim.Zones)
                {
                    if (claimed == zone)
                        return true;
                }
            }

            return false;
        }

        private static void Purge()
        {
            float now = Time.realtimeSinceStartup;

            Blackouts.RemoveAll(claim => claim.Expiry <= now);
        }

        private static string Join(List<BlackoutClaim> claims)
        {
            string[] owners = new string[claims.Count];

            for (int i = 0; i < claims.Count; i++)
                owners[i] = claims[i].Owner;

            return string.Join(", ", owners);
        }
    }
}

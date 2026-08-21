using System;
using System.Collections.Generic;
using Exiled.API.Enums;

namespace AugatonLib.Arbitration
{
    public static class DoorLockRegistry
    {
        private sealed class Lockdown
        {
            public Lockdown(string owner, DoorType[] doors)
            {
                Owner = owner;
                Doors = doors;
            }

            public string Owner { get; }

            public DoorType[] Doors { get; }

            public bool Covers(DoorType door)
            {
                if (Doors is null || Doors.Length == 0)
                    return true;

                foreach (DoorType entry in Doors)
                {
                    if (entry == door)
                        return true;
                }

                return false;
            }
        }

        private static readonly List<Lockdown> Lockdowns = new List<Lockdown>(4);

        public static int Count => Lockdowns.Count;

        public static void Lock(string owner, params DoorType[] doors)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Unlock(owner);
            Lockdowns.Add(new Lockdown(owner, doors));
        }

        public static void LockAll(string owner) => Lock(owner);

        public static void Unlock(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Lockdowns.RemoveAll(lockdown => string.Equals(lockdown.Owner, owner, StringComparison.Ordinal));
        }

        public static bool IsLocked(DoorType door) => IsLocked(door, out _);

        public static bool IsLocked(DoorType door, out string owner)
        {
            owner = null;

            foreach (Lockdown lockdown in Lockdowns)
            {
                if (!lockdown.Covers(door))
                    continue;

                owner = lockdown.Owner;
                return true;
            }

            return false;
        }

        public static void Clear() => Lockdowns.Clear();

        public static string Describe()
        {
            if (Lockdowns.Count == 0)
                return "aucun verrou";

            string[] entries = new string[Lockdowns.Count];

            for (int i = 0; i < Lockdowns.Count; i++)
            {
                Lockdown lockdown = Lockdowns[i];
                int doors = lockdown.Doors is null ? 0 : lockdown.Doors.Length;

                entries[i] = doors == 0
                    ? $"{lockdown.Owner} (toutes)"
                    : $"{lockdown.Owner} ({doors})";
            }

            return string.Join(", ", entries);
        }
    }
}

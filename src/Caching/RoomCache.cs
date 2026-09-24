using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace AugatonLib.Caching
{
    public sealed class RoomCache
    {
        private static readonly RoomType[] Excluded = { RoomType.Unknown, RoomType.Pocket, RoomType.Surface };

        private readonly List<Room> rooms = new List<Room>(64);

        public int Count => rooms.Count;

        public void Rebuild()
        {
            rooms.Clear();

            foreach (Room room in Room.List)
            {
                if (room is null || IsExcluded(room.Type))
                    continue;

                rooms.Add(room);
            }
        }

        public void Clear() => rooms.Clear();

        public Room PickRandom()
        {
            if (rooms.Count == 0)
                Rebuild();

            if (rooms.Count == 0)
                return null;

            if (!Map.IsLczDecontaminated)
                return rooms[UnityEngine.Random.Range(0, rooms.Count)];

            int safe = 0;

            foreach (Room room in rooms)
            {
                if (room.Zone != ZoneType.LightContainment)
                    safe++;
            }

            if (safe == 0)
                return null;

            int target = UnityEngine.Random.Range(0, safe);

            foreach (Room room in rooms)
            {
                if (room.Zone == ZoneType.LightContainment)
                    continue;

                if (target == 0)
                    return room;

                target--;
            }

            return null;
        }

        private static bool IsExcluded(RoomType type)
        {
            foreach (RoomType excluded in Excluded)
            {
                if (excluded == type)
                    return true;
            }

            return false;
        }
    }
}

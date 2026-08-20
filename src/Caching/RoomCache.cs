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

            return rooms.Count == 0 ? null : rooms[UnityEngine.Random.Range(0, rooms.Count)];
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

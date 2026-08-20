using System;
using System.Collections.Generic;
using Exiled.API.Features;

namespace AugatonLib.Caching
{
    public sealed class CooldownTracker
    {
        private readonly Dictionary<string, DateTime> lastUse = new Dictionary<string, DateTime>();

        public bool IsOnCooldown(string userId, float durationSeconds, out double secondsRemaining)
        {
            secondsRemaining = 0d;

            if (durationSeconds <= 0f || string.IsNullOrEmpty(userId))
                return false;

            if (!lastUse.TryGetValue(userId, out DateTime last))
                return false;

            double elapsed = (DateTime.UtcNow - last).TotalSeconds;

            if (elapsed >= durationSeconds)
                return false;

            secondsRemaining = durationSeconds - elapsed;
            return true;
        }

        public bool IsOnCooldown(Player player, float durationSeconds, out double secondsRemaining)
        {
            secondsRemaining = 0d;
            return player is not null && IsOnCooldown(player.UserId, durationSeconds, out secondsRemaining);
        }

        public void Register(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            lastUse[userId] = DateTime.UtcNow;
        }

        public void Register(Player player)
        {
            if (player is not null)
                Register(player.UserId);
        }

        public void Remove(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
                lastUse.Remove(userId);
        }

        public void Remove(Player player)
        {
            if (player is not null)
                Remove(player.UserId);
        }

        public void Clear() => lastUse.Clear();
    }
}

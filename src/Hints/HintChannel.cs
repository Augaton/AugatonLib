using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exiled.API.Features;
using MEC;

namespace AugatonLib.Hints
{
    public sealed class HintChannel
    {
        private static bool serviceAvailable = true;
        private static bool warned;

        private readonly Dictionary<string, object> activeHints = new Dictionary<string, object>();
        private readonly Dictionary<string, CoroutineHandle> timers = new Dictionary<string, CoroutineHandle>();

        public HintChannel(string id, float yCoordinate, int fontSize = 20)
        {
            Id = id;
            YCoordinate = yCoordinate;
            FontSize = fontSize;
        }

        public string Id { get; }

        public float YCoordinate { get; set; }

        public int FontSize { get; set; }

        public static bool ServiceAvailable => serviceAvailable;

        public void Show(Player player, string text, float duration)
        {
            if (player is null || !player.IsConnected || string.IsNullOrEmpty(text))
                return;

            if (duration <= 0f)
                duration = 3f;

            if (serviceAvailable)
            {
                try
                {
                    ShowThroughService(player, text, duration);
                    return;
                }
                catch (Exception e)
                {
                    serviceAvailable = false;
                    activeHints.Clear();

                    if (!warned)
                    {
                        warned = true;
                        Log.Warn($"HintServiceMeow indisponible, retour aux hints natifs : {e.Message}");
                    }
                }
            }

            player.ShowHint(text, duration);
        }

        public void ShowAll(string text, float duration)
        {
            foreach (Player player in Player.List)
                Show(player, text, duration);
        }

        public void Remove(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            Drop(player.UserId);
        }

        public void Clear()
        {
            List<string> userIds = new List<string>(activeHints.Keys);

            foreach (string userId in userIds)
                Drop(userId);

            activeHints.Clear();
            timers.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ShowThroughService(Player player, string text, float duration)
        {
            string userId = player.UserId;

            if (!activeHints.TryGetValue(userId, out object stored) || !(stored is HintServiceMeow.Core.Models.Hints.Hint hint))
            {
                hint = new HintServiceMeow.Core.Models.Hints.Hint
                {
                    Id = Id,
                    YCoordinate = YCoordinate,
                    FontSize = FontSize,
                    Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                    YCoordinateAlign = HintServiceMeow.Core.Enum.HintVerticalAlign.Bottom,
                };

                activeHints[userId] = hint;
                HintServiceMeow.Core.Utilities.PlayerDisplay.Get(player).AddHint(hint);
            }

            hint.Text = text;

            KillTimer(userId);
            timers[userId] = Timing.CallDelayed(duration, () => Drop(userId));
        }

        private void Drop(string userId)
        {
            KillTimer(userId);

            if (!activeHints.TryGetValue(userId, out object stored))
                return;

            activeHints.Remove(userId);

            if (!serviceAvailable)
                return;

            try
            {
                DropThroughService(userId, stored);
            }
            catch (Exception e)
            {
                Log.Debug($"HintChannel.Drop: {e.Message}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DropThroughService(string userId, object stored)
        {
            if (!(stored is HintServiceMeow.Core.Models.Hints.Hint hint))
                return;

            Player player = Player.Get(userId);

            if (player is null || !player.IsConnected)
                return;

            HintServiceMeow.Core.Utilities.PlayerDisplay.Get(player).RemoveHint(hint);
        }

        private void KillTimer(string userId)
        {
            if (!timers.TryGetValue(userId, out CoroutineHandle handle))
                return;

            Timing.KillCoroutines(handle);
            timers.Remove(userId);
        }
    }
}

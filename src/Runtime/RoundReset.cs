using System;
using AugatonLib.Arbitration;
using Exiled.API.Features;
using RoundRestarting;

namespace AugatonLib.Runtime
{
    internal static class RoundReset
    {
        private static bool hooked;

        internal static void Hook()
        {
            if (hooked)
                return;

            hooked = true;
            RoundRestart.OnRestartTriggered += OnRestartTriggered;
        }

        private static void OnRestartTriggered()
        {
            try
            {
                LightArbiter.Forget();
                ScaleArbiter.ForgetAll();
                DoorLockRegistry.Clear();
                GenocideArbiter.Clear();
            }
            catch (Exception e)
            {
                Log.Error($"RoundReset: {e}");
            }
        }
    }
}

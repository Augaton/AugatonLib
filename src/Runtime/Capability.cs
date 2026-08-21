namespace AugatonLib.Runtime
{
    public enum Capability
    {
        Hints,
        FriendlyFire,
        Scale,
        Light,
        DoorLock,
        Departure,
        Genocide,
        Bus,
    }

    public static class CapabilityLabel
    {
        public static string Short(Capability capability)
        {
            switch (capability)
            {
                case Capability.Hints: return "hint";
                case Capability.FriendlyFire: return "ff";
                case Capability.Scale: return "taille";
                case Capability.Light: return "lumiere";
                case Capability.DoorLock: return "porte";
                case Capability.Departure: return "depart";
                case Capability.Genocide: return "genocide";
                case Capability.Bus: return "bus";
                default: return capability.ToString().ToLowerInvariant();
            }
        }
    }
}

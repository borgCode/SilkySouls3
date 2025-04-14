namespace SilkySouls3.Memory
{
    public static class EventFlags
    {
        public const ulong UnlockMidir = 0xE66928;

        public static readonly ulong[] Patches = { 0x554, 0x55F, 0x42C1F10, 0x555 };
    }
}
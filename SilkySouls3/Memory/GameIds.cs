namespace SilkySouls3.Memory
{
    public static class GameIds
    {
        public static class EventFlags
        {
            public const ulong UnlockMidir = 0xE66928;
            public static readonly ulong[] Patches = { 0x554, 0x55F, 0x42C1F10, 0x555 };
            public static readonly int[] ReinforceWeaponFlagRange = { 0xE8, 0xEB };
            public static readonly int[] InfuseWeaponFlagRange = { 0x14B, 0x15B };
        }

        public static class ShopParams
        {
            public static readonly ulong[] ShrineMaiden = { 0x1ADB0, 0x1D45B };
            public static readonly ulong[] Greirat = { 0x1D4C0, 0x1FBCF };
            public static readonly ulong[] Patches = { 0x30D40, 0x3344F };
            public static readonly ulong[] Orbeck = { 0x1FC34, 0x222DF };
            public static readonly ulong[] Cornyx = { 0x22344, 0x249EF };
            public static readonly ulong[] Karla = { 0x24A54, 0x270FF };
            public static readonly ulong[] Transpose = { 0x7530, 0x7918 };
            
        }
    }
}
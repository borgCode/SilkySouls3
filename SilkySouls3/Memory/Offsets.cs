using System;

namespace SilkySouls3.Memory
{
    public static class Offsets
    {
        public static class WorldChrMan
        {
            public static IntPtr Base;
        }

        public static class GameMan
        {
            public static IntPtr Base;
            public const int LastBonfire = 0xACC;
        }

        public static class LuaEventMan
        {
            public static IntPtr Base;
        }
        

        public static class AiTargetingFlags
        {
            public static IntPtr Base;
            public const int Height = 0x4;
            public const int Width = 0x5;
        }
        public static class WorldAiMan
        {
            public static IntPtr Base;
        }

        public static class Hooks
        {
            public static long LastLockedTarget;
            public static long WarpCoordWrite;

        }

        public static class Funcs
        {
            public static long Warp;
        }
    }
}
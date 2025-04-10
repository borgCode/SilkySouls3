using System;

namespace SilkySouls3.Memory
{
    public static class CodeCaveOffsets
    {
       
            public static IntPtr Base;
            public const int LockedTargetPtr = 0x0;
            public const int SaveTargetPtrCode = 0x10;

            public const int CinderPhaseLock = 0x30;

            public const int SavePos1 = 0x70;
            public const int SavePos2 = 0x80;

            public enum WarpHooks
            {
                Coords = 0x90,
                Angle = 0xA0,
                CoordCode = 0xB0,
                AngleCode = 0xD0 
            }
    }
}
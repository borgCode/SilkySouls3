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


        public enum ItemSpawn
        {
            ItemToSpawn = 0x140,
            Param3Addr = 0x150,
            Code = 0x160,
        }

        public enum NoClip
        {
            InAirTimerCode = 0x190,
            ZDirectionVariable = 0x1C0,
            KeyboardCheck = 0x1D0,
            TriggerThreshold = 0x210,
            TriggerCheck = 0x220,
            TriggerCheck2 = 0x260,
            UpdateCoordsCode = 0x2B0
        }

        public const int CamVertUp = 0x3B0;
        public const int ItemLotBase = 0x3D0;
    }
}
using System;

namespace SilkySouls3.Memory
{
    public static class CustomCodeOffsets
    {
        public static IntPtr Base;
        public const int LockedTargetPtr = 0x0;
        public const int SaveTargetPtrCode = 0x10;

        public const int CinderPhaseLock = 0x30;
        

        public enum WarpHooks
        {
            Coords = 0x90,
            Angle = 0xA0,
            CoordCode = 0xB0,
            AngleCode = 0xD0
        }


        public enum ItemSpawn
        {
            QtyAdjustFlag = 0x140,
            MaxQty = 0x144,
            ItemToSpawn = 0x150,
            Param3Addr = 0x160,
            Code = 0x170,
        }

        
        public const int CamVertUp = 0x3B0;
        public const int ItemLotBase = 0x3D0;
        public const int GetEventResult = 0x400;

        public const int DisableAllExceptTarget = 0x460;

        public enum Butterfly
        {
            LeftSideAnimationId = 0x500,
            RightSideAnimationId = 0x510,
            Code = 0x520
        }

        public const int CinderSoulmassRemoval = 0x700;

        public const int StoredChrInsByEntityId = 0x800;

        public const int SetReminderCode = 0x900;
        public const int ReminderText = 0x950;

        public const int EzStateTalkParams = 0xA00;
        public const int EzStateTalkCode = 0xB00;

        
        public const int ZDirection = 0x1000;
        public const int SpeedScale = 0x1004;
        public const int InAirTimerCode = 0x1050;
        public const int KeyboardCode = 0x1100;
        public const int TriggersCode = 0x1200;
        public const int UpdateCoordsCode = 0x1300;
        
        

    }
}
using System;

namespace SilkySouls3.Memory
{
    public static class Offsets
    {
        public static class WorldChrMan
        {
            public static IntPtr Base;
            public const int PlayerIns = 0x80;

            public enum PlayerInsOffsets
            {
                ChrFlags1 = 0x1ED8,
                Modules = 0x1F90,
            }

            public enum ChrFlag1BitFlag
            {
                DisableAi = 75
            }
            
            public enum Modules
            {
                ChrDataModule = 0x18,
                ChrResistModule = 0x20,
                ChrBehaviorModule = 0x28,
                ChrEventModule = 0x58
            }

            public enum ChrDataModule
            {
                Hp = 0xD8,
                MaxHp = 0xDC,
                ChrFlags2 = 0x1C0
                
            }

            public enum ChrFlags2 : byte
            {
                NoDamage = 1 << 1,
                InfiniteStam = 1 << 4,
            }

            public enum ChrResistModule
            {
                PoisonCurrent = 0x10,
                ToxicCurrent = 0x14,
                BleedCurrent = 0x18,
                FrostCurrent = 0x20,
                PoisonMax = 0x24,
                ToxicMax = 0x28,
                BleedMax = 0x2C,
                FrostMax = 0x34
            }
            
//TODO look into replacing behavior module and current anim with better
            public const int CurrentAnimationOffset = 0x898;
            public const int ForceAnimationOffset = 0x20;
            
        }

        public static class GameMan
        {
            public static IntPtr Base;
            public const int LastBonfire = 0xACC;
        }

        public static class DamageMan
        {
            public static IntPtr Base;
            public const int HitboxView = 0x30;
        }

        public static class SoloParamRepo
        {
            public static IntPtr Base;
            public const int ParamResCap = 0x4A8;
            public const int SpEffectPtr1 = 0x68;
            public const int SpEffectPtr2 = 0x68;
            public const int CinderSoulmass = 0x126730;
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

        public static class EnemyIns
        {
            public const int EnemyCtrl = 0x50;
            public const int ActionCtrl = 0x48;
            public const int CurrentPhaseOffset = 0xF0;
            
            public const int ComManipulator = 0x58;
            public const int EnemyId = 0x390;
            
            public const int AiIns = 0x320;

            public enum AiInsOffsets
            {
                SpEffectPtr = 0x20,
                LuaNumbers = 0x6BC,
            }

            public enum SpEffectImmunityOffsets
            {
                Poison = 0x60,
                Toxic = 0x64,
                Bleed = 0x170,
                FrostBite = 0x178
            }
            
            public enum LuaNumbers
            
            {
                Gwyn5HitComboNumberIndex = 0,
                GwynLightningRainNumberIndex = 1,
                PhaseTransitionCounterNumberIndex = 2
            }

        }

        public static class Hooks
        {
            public static long LastLockedTarget;
            public static long WarpCoordWrite;
            public static long AddSubGoal;

        }

        public static class Funcs
        {
            public static long Warp;
        }
    }
}
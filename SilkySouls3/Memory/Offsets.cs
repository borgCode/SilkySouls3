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
                ChrFlags1 = 0x1EE8,
                Modules = 0x1F90,
            }

            public enum ChrFlag1BitFlag
            {
                DisableAi = 11
            }

            public enum Modules
            {
                ChrDataModule = 0x18,
                ChrResistModule = 0x20,
                ChrBehaviorModule = 0x28,
                ChrSuperArmorModule = 0x40,
                ChrEventModule = 0x58,
                ChrPhysicsModule = 0x68
            }
            public const int ForceAnimationOffset = 0x20;

            public const int CsChrProxy = 0xA8;
            public const int CsHkCharacterProxy = 0x40;
            public const int TargetCoordsOffset = 0x70;

            public enum ChrDataModule
            {
                Hp = 0xD8,
                MaxHp = 0xDC,
                ChrFlags2 = 0x1C0
            }

            public enum ChrFlags2 : byte
            {
                NoDamage = 1 << 1,
                NoDeath = 1 << 2,
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

            public enum ChrBehaviorModule
            {
                AnimSpeed = 0xA58,
                CurrentAnimation = 0x898,
            }

            public enum ChrSuperArmorModule
            {
                Poise = 0x28,
                MaxPoise = 0x2C,
                PoiseTimer = 0x34,
            }

            public enum ChrPhysicsModule
            {
                Angle = 0x74,
                Coords = 0x80,
            }
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

        public static class DebugFlags
        {
            public static IntPtr Base;
            public const int NoDeath = 0x0;
            public const int OneShot = 0x1;
            public const int InfiniteStam = 0x2;
            public const int InfiniteFp = 0x3;
            public const int InfiniteArrows = 0x4;
            public const int Invisible = 0x6;
            public const int Silent = 0x7;
            public const int AllNoDeath = 0x8;
            public const int AllNoDamage = 0x9;
            public const int DisableAllAi = 0xD;
        }

        public static class DebugEvent
        {
            public static IntPtr Base;
            public const int EventDraw = 0xA8;
        }

        public static class SoloParamRepo
        {
            public static IntPtr Base;
            public const int ParamResCap = 0x4A8;
            public const int SpEffectPtr1 = 0x68;
            public const int SpEffectPtr2 = 0x68;
            public const int CinderSoulmass = 0x126730;
        }

        public static class MenuMan
        {
            public static IntPtr Base;

            public enum MenuManOffsets
            {
                LoadedFlag = 0x28C,
            }
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
            
            public const int CurrentPhaseOffset = 0x1FF0;
            
        }

        public static class Patches
        {
            public static long RepeatAct;
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
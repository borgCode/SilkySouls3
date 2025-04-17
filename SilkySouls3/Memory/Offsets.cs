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
                PadMan = 0x58,
                CharFlags1 = 0x1EE8,
                Modules = 0x1F90,
            }
            
            public enum ChrFlag1BitFlag
            {
                DisableAi = 11,
                NoGoodsConsume = 19
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
                InfinitePoise = 0x10,
                Poise = 0x28,
                MaxPoise = 0x2C,
                PoiseTimer = 0x34,
            }

            public const byte InfinitePoise = 1 << 0;

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

        public static class EventFlagMan
        {
            public static IntPtr Base;
            public const int CoiledSword = 0x5A0F;
            public const byte CoiledSwordBitFlag = 1 << 2;
            public const int Firelink = 0x5A03;
            public const byte FirelinkBitFlag = 1 << 7;
        }

        public static class GameDataMan
        {
            public static IntPtr Base;
            public const int PlayerGameData = 0x10;

            public enum Stats
            {
                Vigor = 0x44,
                Attunement = 0x48,
                Endurance = 0x4C,
                Strength = 0x50,
                Dexterity = 0x54,
                Intelligence = 0x58,
                Faith = 0x5C,
                Luck = 0x60,
                Vitality = 0x6C,
                SoulLevel = 0x70,
                Souls = 0x74,
                TotalSouls = 0x78
            }
            public const int NewGame = 0x78;
            public const int InGameTime = 0xA4;
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
            public const int DisableEvent = 0xD4;
        }

        public static class SoloParamRepo
        {
            public static IntPtr Base;
            public const int SpEffectParamResCap = 0x4A8;
            public const int SpEffectPtr1 = 0x68;
            public const int SpEffectPtr2 = 0x68;
            public const int CinderSoulmass = 0x126730;

            public const int CamParamResCap = 0x928;
            public const int CamPtr1 = 0x68;
            public const int CamPtr2 = 0x68;
            public const int CamFov = 0xD8C;
        }

        public static class MenuMan
        {
            public static IntPtr Base;

            public enum MenuManOffsets
            {
                QuitOut = 0x250,
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
        
        public static class MapItemMan
        {
            public static IntPtr Base;
        }

        public static class HitIns
        {
            public static IntPtr Base;
            public const int LowHit = 0x0;
            public const int HighHit = 0x1;
            public const int ChrRagdoll = 0x3;
        }
        
        public static class EnemyIns
        {
            
            public const int ComManipulator = 0x58;

            public enum ComManipOffsets
            {
                EnemyId = 0x390,
                AiIns = 0x320
            }
            
            public enum AiInsOffsets
            {
                AiFunc = 0x8,
                SpEffectPtr = 0x20,
                LuaNumbers = 0x6BC,
            }

            public const int ForceActPtr = 0x8;
            public const int ForceActOffset = 0xB681;

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

        public static class FieldArea
        {
            public static IntPtr Base;
            public const int GameRend = 0x18;
            public const int DbgFreeCamMode = 0xE0;
            public const int DbgFreeCam = 0xE8;
            public const int DbgFreeCamCoords = 0x40;
            public const int ChrCam = 0x28;
            public const int ChrExFollowCam = 0x60;
            public const int CameraDownLimit = 0x200;
        }
        
        public static class GroupMask
        {
            public static IntPtr Base;
            public const int Map = 0x0;
            public const int Obj = 0x1;
            public const int Chr = 0x2;
            public const int Sfx = 0x3;
        }

        public static class UserInputManager
        {
            public static IntPtr Base;
            public const int SteamInputEnum = 0x24B;
        }

        public static class Patches
        {
            public static IntPtr NoLogo;
            public static IntPtr RepeatAct;
            public static IntPtr GameSpeed;
            public static IntPtr InfiniteDurability;
            public static IntPtr PlayerSoundView;
            public static IntPtr DebugFont;
            public static IntPtr NoRoll;
            public static IntPtr TargetingView;
            public static IntPtr FreeCam;
        }
        
        
        public static class Hooks
        {
            public static long LastLockedTarget;
            public static long WarpCoordWrite;
            public static long AddSubGoal;
            public static long InAirTimer;
            public static long NoClipKeyboard;
            public static long NoClipTriggers;
            public static long NoClipTriggers2;
            public static long NoClipUpdateCoords;
            public static long CameraUpLimit;

        }

        public static class Funcs
        {
            public static long Warp;
            public static long ItemSpawn;
            public static long SetEvent;
            public static long Travel;
            public static long LevelUp;
            public static long ReinforceWeapon;
            public static long InfuseWeapon;
            public static long Repair;
            public static long AllotEstus;
            public static long Attunement;
            public static long RegularShop;
            public static long Transpose;
            public static long CombineMenuFlagAndEventFlag;
        }
    }
}
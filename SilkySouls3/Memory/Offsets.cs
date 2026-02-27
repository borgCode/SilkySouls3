using System;
using SilkySouls3.Enums;
using SilkySouls3.Utilities;
using static SilkySouls3.Enums.GameVersion;

namespace SilkySouls3.Memory
{
    public static class Offsets
    {
        private static GameVersion? _version;

        public static GameVersion Version => _version
                                             ?? Version1_15_0_0;

        public static bool Initialize(string fileVersion, nint moduleBase)
        {
            _version = fileVersion switch
            {
                var v when v.StartsWith("1.3.2.") => Version1_3_2_0,
                var v when v.StartsWith("1.4.1.") => Version1_4_1_0,
                var v when v.StartsWith("1.4.2.") => Version1_4_2_0,
                var v when v.StartsWith("1.4.3.") => Version1_4_3_0,
                var v when v.StartsWith("1.5.0.") => Version1_5_0_0,
                var v when v.StartsWith("1.5.1.") => Version1_5_1_0,
                var v when v.StartsWith("1.6.0.") => Version1_6_0_0,
                var v when v.StartsWith("1.7.0.") => Version1_7_0_0,
                var v when v.StartsWith("1.8.0.") => Version1_8_0_0,
                var v when v.StartsWith("1.9.0.") => Version1_9_0_0,
                var v when v.StartsWith("1.10.0.") => Version1_10_0_0,
                var v when v.StartsWith("1.11.0.") => Version1_11_0_0,
                var v when v.StartsWith("1.12.0.") => Version1_12_0_0,
                var v when v.StartsWith("1.13.0.") => Version1_13_0_0,
                var v when v.StartsWith("1.14.0.") => Version1_14_0_0,
                var v when v.StartsWith("1.15.0.") => Version1_15_0_0,
                var v when v.StartsWith("1.15.1.") => Version1_15_1_0,
                var v when v.StartsWith("1.15.2.") => Version1_15_2_0,
                _ => null
            };

            if (!_version.HasValue)
            {
                MsgBox.Show(
                    $@"Unknown patch version: {fileVersion}, please report it on GitHub",
                    "Unknown patch version");
                return false;
            }


            _moduleBase = moduleBase;
            InitializeBaseAddresses(moduleBase);
            return true;
        }

        private static nint _moduleBase;

        public static class WorldChrManImp
        {
            public static nint Base;
            public const int PlayerIns = 0x80;

            public static int DeathCam => Version switch
            {
                < Version1_12_0_0 => 0x88,
                _ => 0x90
            };

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
        }

        public static class ChrIns
        {
            public const int ChrModel = 0x48;

            public static class ChrModelOffsets
            {
                public const int ChrModelPrimMask = 0x78;
            }

            public const int ChrPrimMaskMirrorOffset = 0x10A0;
            public const int ChrPrimMaskOffset = 0x1FF0;
            public static readonly BitFlag IsTaePrimMaskActive = new BitFlag(0x1A09, 0);

            public const int SpecialEffect = 0x11C8;

            public static class SpecialEffectOffsets
            {
                public const int Head = 0x8;
            }

            public static class SpEffectEntry
            {
                public const int TimeLeft = 0x0;
                public const int Duration = 0x4;
                public const int Id = 0x60;
                public const int SpEffectParam = 0x68;
                public const int Next = 0x78;
            }

            public static class SpEffectParamData
            {
                public const int StateInfo = 0x156;
            }

            public static readonly int[] CurrentBlockId = [0x1AC8, 0x18, 0x4E8];

            public static int Modules => Version switch
            {
                Version1_12_0_0 => 0x1F88,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 or Version1_15_1_0 or Version1_15_2_0 => 0x1F90,
                _ => 0x1F80,
            };

            public static int Flags => Version switch
            {
                < Version1_12_0_0 => 0x1ED8,
                Version1_12_0_0 => 0x1EE0,
                _ => 0x1EE8
            };

            public enum FlagsBits
            {
                NoHit = 1 << 5,
                NoAttack = 1 << 6,
                NoMove = 1 << 7,
                DisableAi = 1 << 11,
                NoGoodsConsume = 1 << 19,
            }

            public static readonly int[] ChrTimeActModule = [Modules, 0x10];
            public static readonly int[] ChrDataModule = [Modules, 0x18];
            public static readonly int[] ChrResistModule = [Modules, 0x20];
            public static readonly int[] ChrBehaviorModule = [Modules, 0x28];
            public static readonly int[] ChrSuperArmorModule = [Modules, 0x40];
            public static readonly int[] ChrEventModule = [Modules, 0x58];
            public static readonly int[] ChrPhysicsModule = [Modules, 0x68];

            public static class ChrTimeActOffsets
            {
                public const int CurrentAnimationId = 0xD0;
            }

            public static class ChrDataOffsets
            {
                public const int Hp = 0xD8;
                public const int MaxHp = 0xDC;
                public const int Mp = 0xE4;
                public const int MaxMp = 0xE8;
                public const int Sp = 0xF0;
                public static readonly BitFlag NoDamage = new(0x1C0, 1 << 1);
            }

            public static class ChrBehaviorOffsets
            {
                public static int AnimationSpeed => Version <= Version1_8_0_0 ? 0xA38 : 0xA58;
            }

            public static class ChrSuperArmorOffsets
            {
                public static readonly BitFlag InfinitePoise = new(0x10, 1 << 0);
                public const int Poise = 0x28;
                public const int MaxPoise = 0x2C;
                public const int PoiseTimer = 0x34;
            }

            public static class ChrEventOffsets
            {
                public const int AnimationRequest = 0x20;
            }

            public static class ChrPhysicsOffsets
            {
                public const int Angle = 0x74;
                public const int Position = 0x80;
                public const int PrevPosition = 0x90;
                public const int PhysicsDirty = 0xA1;
                public const int HitRadius = 0x370;
            }

            public static readonly int[] AiThink = [0x58, 0x320];

            public static class AiThinkOffsets
            {
                public const int NpcParam = 0x20;
                public const int LuaNumbers = 0x6BC;
                public const int TargetingSystem = 0x7AB8;
                public const int ForceActIdx = 0xB681;
                public const int LastActIdx = 0xB682;
            }

            public static class NpcParamOffsets
            {
                public const int PoisonImmunity = 0x60;
                public const int ToxicImmunity = 0x64;
                public const int BleedImmunity = 0x170;
                public const int FrostBiteImmunity = 0x178;
                public const int Absorptions = 0x19C;
            }

            public static class TargetingSystemOffsets
            {
                public const int TargetingView = 0x3C;
            }
        }

        public static class GameMan
        {
            public static nint Base;
        }

        public static class DamageMan
        {
            public static nint Base;
            public const int HitboxView = 0x30;
        }

        public static class EventFlagMan
        {
            public static nint Base;
            public const int CoiledSword = 0x5A0F;
            public const byte CoiledSwordBitFlag = 1 << 2;
        }

        public static class GameDataMan
        {
            public static nint Base;
            public const int PlayerGameData = 0x10;

            public static class PlayerGameDataOffsets
            {
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

                public const int EquipInventory = 0x3D0;
                public const int StorageInventory = 0x7B0;
            }
            
        }

        public static class DebugFlags
        {
            public static nint Base;
            public const int NoDeath = 0x0;
            public const int OneShot = 0x1;
            public const int InfiniteStam = 0x2;
            public const int InfiniteFp = 0x3;
            public const int InfiniteArrows = 0x4;
            public const int Invisible = 0x6;
            public const int Silent = 0x7;
            public const int AllNoDeath = 0x8;
            public const int AllNoDamage = 0x9;
            public const int AllNoAttack = 0xB;
            public const int AllNoMove = 0xC;
            public const int AllNoUpdate = 0xD;
        }

        public static class SprjDbgEvent
        {
            public static nint Base;
            public const int EventDraw = 0xA8;
            public const int DisableEvent = 0xD4;
        }

        public static class SoloParamRepo
        {
            public static nint Base;
        }

        public static class MenuMan
        {
            public static nint Base;

            public const int Quitout = 0x250;
        }

        public static class LuaEventMan
        {
            public static nint Base;
        }

        public static class AiTargetingFlags
        {
            public static nint Base;
            public const int Height = 0x4;
            public const int Width = 0x5;
        }

        public static class MapItemMan
        {
            public static nint Base;
        }

        public static class HitFlags
        {
            public static nint Base;
            public const int LowHit = 0x0;
            public const int HighHit = 0x1;
            public const int ChrRagdoll = 0x3;
        }

        public static class WorldObjManImpl
        {
            public static nint Base;
        }

        public static class CSDlc
        {
            public static nint Base;
            public const int Dlc1 = 0x11;
            public const int Dlc2 = 0x12;
        }

        public static class FieldArea
        {
            public static nint Base;
            public const int WorldInfoOwnerPtr = 0x10;
            public const int GameRend = 0x18;
            public const int CurrentBlockIdx = 0x20;
            public const int CamMode = 0xE0;
            public const int DbgFreeCam = 0xE8;
            public const int DbgFreeCamCoords = 0x40;
            public const int ChrCam = 0x28;
            public const int ChrExFollowCam = 0x60;
            public const int CameraDownLimit = 0x200;

            public static class WorldInfoOwner
            {
                public const int WorldBlockInfoCount = 0x18;
                public const int Blocks = 0x20;
            }

            public static class WorldBlockInfo
            {
                public const int CeremonyId = 0x64;
                public const int MapVariant = 0x68;
            }
        }

        public static class GroupMask
        {
            public static nint Base;
            public const int Map = 0x0;
            public const int Obj = 0x1;
            public const int Chr = 0x2;
            public const int Sfx = 0x3;
        }

        public static class UserInputManager
        {
            public static nint Base;
            public const int SteamInputEnum = 0x24B;
        }

        public static class SprjFlipper
        {
            public static nint Base;
            public const int Fps = 0x354;
            public const int DebugFpsToggle = 0x358;
        }

        public static class SprjBulletManager
        {
            public static nint Base;
        }

        public static class FD4PadManager
        {
            public static nint Base;
        }

        public static nint GameSpeed;
        public static nint DrawNavigationPath;

        public static class Hooks
        {
            public static nint LastLockedTarget;
            public static nint WarpCoords;
            public static nint WarpAngle;
            public static nint AddSubGoal;
            public static nint InAirTimer;
            public static nint NoClipKeyboard;
            public static nint NoClipTriggers;
            public static nint NoClipUpdateCoords;
            public static nint CameraUpLimit;
            public static nint ItemLotBase;
            public static nint AddSubGoalDsa;
            public static nint SoulmassStaggerRemoval;
            public static nint LoadScreenItemName;
            public static nint DisableAllExceptTarget;
        }

        public static class Patches
        {
            public static nint NoLogo;
            public static nint RepeatAct;
            public static nint InfiniteDurability;
            public static nint PlayerSoundView;
            public static nint DebugFont;
            public static nint NoRoll;
            public static nint DbgDrawFlag;
            public static nint IsWorldPaused;
            public static nint AccessFullShop;
            public static nint DefaultSoundVolWrite;
            public static nint StartMenuMusicPatch;
        }

        public static class Functions
        {
            public static nint BonfireWarp;
            public static nint ItemSpawn;
            public static nint SetEvent;
            public static nint GetEvent;
            public static nint BreakAllObjects;
            public static nint RestoreAllObjects;
            public static nint ApplySpEffect;
            public static nint HasSpEffectId;
            public static nint StopMusic;
            public static nint Rest;
            public static nint ChrInsByEntityId;
            public static nint SpawnSfxSimple;
            public static nint DestroyFxInner;
            public static nint RegisterPrismStoneSfx;
            public static nint EzStateExternalEventTempCtor;
            public static nint ExecuteTalkCommand;
            public static nint GetYMovement;
            public static nint GetXMovement;
            public static nint MatrixTransformVector;
            public static nint GetPad;
            public static nint StartMenuMusic;
            public static nint FindAndRemoveSpEffect;
            public static nint GetItemQuantity;
        }

        private static void InitializeBaseAddresses(nint moduleBase)
        {
            WorldChrManImp.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46C4AA8,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46C5DC8,
                Version1_5_0_0 => 0x46C9EC8,
                Version1_5_1_0 => 0x46C8EC8,
                Version1_6_0_0 => 0x46C9F28,
                Version1_7_0_0 => 0x46CE768,
                Version1_8_0_0 => 0x472CF58,
                Version1_9_0_0 or Version1_10_0_0 => 0x472D098,
                Version1_11_0_0 => 0x4760398,
                Version1_12_0_0 => 0x4763518,
                Version1_13_0_0 => 0x4766D18,
                Version1_14_0_0 or Version1_15_0_0 => 0x4768E78,
                Version1_15_1_0 or Version1_15_2_0 => 0x477FDB8,
                _ => 0
            };

            GameMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x469F710,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46A0A30,
                Version1_5_0_0 => 0x46A4B30,
                Version1_5_1_0 => 0x46A3B30,
                Version1_6_0_0 => 0x46A4B90,
                Version1_7_0_0 => 0x46A93D0,
                Version1_8_0_0 => 0x4707B90,
                Version1_9_0_0 or Version1_10_0_0 => 0x4707CD0,
                Version1_11_0_0 => 0x473AFD0,
                Version1_12_0_0 => 0x473E150,
                Version1_13_0_0 => 0x4741950,
                Version1_14_0_0 or Version1_15_0_0 => 0x4743AB0,
                Version1_15_1_0 or Version1_15_2_0 => 0x475AC00,
                _ => 0
            };

            DebugFlags.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46C4B98,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46C5EB8,
                Version1_5_0_0 => 0x46C9FB8,
                Version1_5_1_0 => 0x46C8FB8,
                Version1_6_0_0 => 0x46CA018,
                Version1_7_0_0 => 0x46CE858,
                Version1_8_0_0 => 0x472D049,
                Version1_9_0_0 or Version1_10_0_0 => 0x472D189,
                Version1_11_0_0 => 0x4760488,
                Version1_12_0_0 => 0x4763608,
                Version1_13_0_0 => 0x4766E08,
                Version1_14_0_0 or Version1_15_0_0 => 0x4768F68,
                Version1_15_1_0 => 0x477FEA8,
                Version1_15_2_0 => 0x477FE94,
                _ => 0
            };

            SprjDbgEvent.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4696A68,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4697D88,
                Version1_5_0_0 => 0x469BE88,
                Version1_5_1_0 => 0x469AE88,
                Version1_6_0_0 => 0x469BEE8,
                Version1_7_0_0 => 0x46A0728,
                Version1_8_0_0 => 0x46FEE88,
                Version1_9_0_0 or Version1_10_0_0 => 0x46FEFC8,
                Version1_11_0_0 => 0x4732298,
                Version1_12_0_0 => 0x4735418,
                Version1_13_0_0 => 0x4738C18,
                Version1_14_0_0 or Version1_15_0_0 => 0x473AD78,
                Version1_15_1_0 or Version1_15_2_0 => 0x4751EB8,
                _ => 0
            };


            DamageMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46C27B0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46C3AD0,
                Version1_5_0_0 => 0x46C7BD0,
                Version1_5_1_0 => 0x46C6BD0,
                Version1_6_0_0 => 0x46C7C30,
                Version1_7_0_0 => 0x46CC470,
                Version1_8_0_0 => 0x472AC60,
                Version1_9_0_0 or Version1_10_0_0 => 0x472ADA0,
                Version1_11_0_0 => 0x475E0A0,
                Version1_12_0_0 => 0x4761220,
                Version1_13_0_0 => 0x4764A20,
                Version1_14_0_0 or Version1_15_0_0 => 0x4766B80,
                Version1_15_1_0 or Version1_15_2_0 => 0x477DAC0,
                _ => 0
            };


            EventFlagMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4697B18,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4698E38,
                Version1_5_0_0 => 0x469CF38,
                Version1_5_1_0 => 0x469BF38,
                Version1_6_0_0 => 0x469CF98,
                Version1_7_0_0 => 0x46A17D8,
                Version1_8_0_0 => 0x46FFF38,
                Version1_9_0_0 or Version1_10_0_0 => 0x4700078,
                Version1_11_0_0 => 0x4733348,
                Version1_12_0_0 => 0x47364C8,
                Version1_13_0_0 => 0x4739CC8,
                Version1_14_0_0 or Version1_15_0_0 => 0x473BE28,
                Version1_15_1_0 or Version1_15_2_0 => 0x4752F68,
                _ => 0
            };

            GameDataMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x469BDF8,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x469D118,
                Version1_5_0_0 => 0x46A1218,
                Version1_5_1_0 => 0x46A0218,
                Version1_6_0_0 => 0x46A1278,
                Version1_7_0_0 => 0x46A5AB8,
                Version1_8_0_0 => 0x4704268,
                Version1_9_0_0 or Version1_10_0_0 => 0x47043A8,
                Version1_11_0_0 => 0x4737698,
                Version1_12_0_0 => 0x473A818,
                Version1_13_0_0 => 0x473E018,
                Version1_14_0_0 or Version1_15_0_0 => 0x4740178,
                Version1_15_1_0 or Version1_15_2_0 => 0x47572B8,
                _ => 0
            };


            SoloParamRepo.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46DE068,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46DF388,
                Version1_5_0_0 => 0x46E3498,
                Version1_5_1_0 => 0x46E2498,
                Version1_6_0_0 => 0x46E34F8,
                Version1_7_0_0 => 0x46E7D48,
                Version1_8_0_0 => 0x4746668,
                Version1_9_0_0 or Version1_10_0_0 => 0x47467A8,
                Version1_11_0_0 => 0x4779D48,
                Version1_12_0_0 => 0x477CEC8,
                Version1_13_0_0 => 0x47806D8,
                Version1_14_0_0 or Version1_15_0_0 => 0x4782838,
                Version1_15_1_0 => 0x4798118,
                Version1_15_2_0 => 0x4798108,
                _ => 0
            };

            MenuMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46A7F60,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46A9280,
                Version1_5_0_0 => 0x46AD380,
                Version1_5_1_0 => 0x46AC380,
                Version1_6_0_0 => 0x46AD3E0,
                Version1_7_0_0 => 0x46B1C18,
                Version1_8_0_0 => 0x47103D8,
                Version1_9_0_0 or Version1_10_0_0 => 0x4710518,
                Version1_11_0_0 => 0x4743808,
                Version1_12_0_0 => 0x4746988,
                Version1_13_0_0 => 0x474A188,
                Version1_14_0_0 or Version1_15_0_0 => 0x474C2E8,
                Version1_15_1_0 or Version1_15_2_0 => 0x4763258,
                _ => 0
            };


            LuaEventMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46966C8,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46979E8,
                Version1_5_0_0 => 0x469BAE8,
                Version1_5_1_0 => 0x469AAE8,
                Version1_6_0_0 => 0x469BB48,
                Version1_7_0_0 => 0x46A0388,
                Version1_8_0_0 => 0x46FEAE8,
                Version1_9_0_0 or Version1_10_0_0 => 0x46FEC28,
                Version1_11_0_0 => 0x4731EE8,
                Version1_12_0_0 => 0x4735068,
                Version1_13_0_0 => 0x4738868,
                Version1_14_0_0 or Version1_15_0_0 => 0x473A9C8,
                Version1_15_1_0 or Version1_15_2_0 => 0x4751B08,
                _ => 0
            };

            AiTargetingFlags.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46957C0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4696AE0,
                Version1_5_0_0 => 0x469ABE0,
                Version1_5_1_0 => 0x4699BE0,
                Version1_6_0_0 => 0x469AC40,
                Version1_7_0_0 => 0x469F480,
                Version1_8_0_0 => 0x46FDBE0,
                Version1_9_0_0 or Version1_10_0_0 => 0x46FDD20,
                Version1_11_0_0 => 0x4730FE0,
                Version1_12_0_0 => 0x4734160,
                Version1_13_0_0 => 0x4737960,
                Version1_14_0_0 or Version1_15_0_0 => 0x4739AC0,
                Version1_15_1_0 or Version1_15_2_0 => 0x4750C00,
                _ => 0
            };

            MapItemMan.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46ADF60,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46AF280,
                Version1_5_0_0 => 0x46B3380,
                Version1_5_1_0 => 0x46B2380,
                Version1_6_0_0 => 0x46B33E0,
                Version1_7_0_0 => 0x46B7C20,
                Version1_8_0_0 => 0x47163F0,
                Version1_9_0_0 or Version1_10_0_0 => 0x4716530,
                Version1_11_0_0 => 0x4749820,
                Version1_12_0_0 => 0x474C9A0,
                Version1_13_0_0 => 0x47501A0,
                Version1_14_0_0 or Version1_15_0_0 => 0x4752300,
                Version1_15_1_0 or Version1_15_2_0 => 0x4769240,
                _ => 0
            };

            HitFlags.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46C289C,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46C3BBC,
                Version1_5_0_0 => 0x46C7CBC,
                Version1_5_1_0 => 0x46C6CBC,
                Version1_6_0_0 => 0x46C7D1C,
                Version1_7_0_0 => 0x46CC55C,
                Version1_8_0_0 => 0x472AD4C,
                Version1_9_0_0 or Version1_10_0_0 => 0x472AE8C,
                Version1_11_0_0 => 0x475E18C,
                Version1_12_0_0 => 0x476130C,
                Version1_13_0_0 => 0x4764B0C,
                Version1_14_0_0 or Version1_15_0_0 => 0x4766C6C,
                Version1_15_1_0 or Version1_15_2_0 => 0x477DBAC,
                _ => 0
            };

            WorldObjManImpl.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46A0E28,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46A2148,
                Version1_5_0_0 => 0x46A6248,
                Version1_5_1_0 => 0x46A5248,
                Version1_6_0_0 => 0x46A62A8,
                Version1_7_0_0 => 0x46AAB20,
                Version1_8_0_0 => 0x47092A8,
                Version1_9_0_0 or Version1_10_0_0 => 0x47093E8,
                Version1_11_0_0 => 0x473C6E8,
                Version1_12_0_0 => 0x473F868,
                Version1_13_0_0 => 0x4743068,
                Version1_14_0_0 or Version1_15_0_0 => 0x47451C8,
                Version1_15_1_0 or Version1_15_2_0 => 0x475C170,
                _ => 0
            };

            FieldArea.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x469F6D8,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46A09F8,
                Version1_5_0_0 => 0x46A4AF8,
                Version1_5_1_0 => 0x46A3AF8,
                Version1_6_0_0 => 0x46A4B58,
                Version1_7_0_0 => 0x46A9398,
                Version1_8_0_0 => 0x4707B58,
                Version1_9_0_0 or Version1_10_0_0 => 0x4707C98,
                Version1_11_0_0 => 0x473AFA0,
                Version1_12_0_0 => 0x473E120,
                Version1_13_0_0 => 0x4741920,
                Version1_14_0_0 or Version1_15_0_0 => 0x4743A80,
                Version1_15_1_0 or Version1_15_2_0 => 0x475ABD0,
                _ => 0
            };

            GroupMask.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x44BA000,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x44BB000,
                Version1_5_0_0 or Version1_6_0_0 => 0x44BF010,
                Version1_5_1_0 => 0x44BE010,
                Version1_7_0_0 => 0x44C2EC8,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x451B608,
                Version1_11_0_0 => 0x454DCE0,
                Version1_12_0_0 => 0x4550CF0,
                Version1_13_0_0 => 0x4553CF0,
                Version1_14_0_0 or Version1_15_0_0 => 0x4555CF0,
                Version1_15_1_0 or Version1_15_2_0 => 0x456CBA8,
                _ => 0
            };

            UserInputManager.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x48A9968,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x48AAC70,
                Version1_5_0_0 => 0x48AED80,
                Version1_5_1_0 => 0x48ADD80,
                Version1_6_0_0 => 0x48AEDF0,
                Version1_7_0_0 => 0x48B3670,
                Version1_8_0_0 => 0x49127F0,
                Version1_9_0_0 or Version1_10_0_0 => 0x4912930,
                Version1_11_0_0 => 0x4945EC8,
                Version1_12_0_0 => 0x4949058,
                Version1_13_0_0 => 0x494C878,
                Version1_14_0_0 or Version1_15_0_0 => 0x494E9D8,
                Version1_15_1_0 => 0x49644C8,
                Version1_15_2_0 => 0x49644B8,
                _ => 0
            };


            SprjFlipper.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x47E3940,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x47E4C60,
                Version1_5_0_0 => 0x47E8D70,
                Version1_5_1_0 => 0x47E7D70,
                Version1_6_0_0 => 0x47E8DD0,
                Version1_7_0_0 => 0x47ED650,
                Version1_8_0_0 => 0x484BFD0,
                Version1_9_0_0 or Version1_10_0_0 => 0x484C110,
                Version1_11_0_0 => 0x487F930,
                Version1_12_0_0 => 0x4882AC0,
                Version1_13_0_0 => 0x48862E0,
                Version1_14_0_0 or Version1_15_0_0 => 0x4888440,
                Version1_15_1_0 => 0x489DD20,
                Version1_15_2_0 => 0x489DD10,
                _ => 0
            };


            SprjBulletManager.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x46CE988,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46CFCA8,
                Version1_5_0_0 => 0x46D3DA8,
                Version1_5_1_0 => 0x46D2DA8,
                Version1_6_0_0 => 0x46D3E08,
                Version1_7_0_0 => 0x46D8648,
                Version1_8_0_0 => 0x4736E48,
                Version1_9_0_0 or Version1_10_0_0 => 0x4736F88,
                Version1_11_0_0 => 0x476A298,
                Version1_12_0_0 => 0x476D418,
                Version1_13_0_0 => 0x4770C18,
                Version1_14_0_0 or Version1_15_0_0 => 0x4772D78,
                Version1_15_1_0 => 0x4789CB8,
                Version1_15_2_0 => 0x4789CA8,
                _ => 0
            };

            FD4PadManager.Base = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x48A9970,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x48AAC78,
                Version1_5_0_0 => 0x48AED88,
                Version1_5_1_0 => 0x48ADD88,
                Version1_6_0_0 => 0x48AEDF8,
                Version1_7_0_0 => 0x48B3680,
                Version1_8_0_0 => 0x4912800,
                Version1_9_0_0 or Version1_10_0_0 => 0x4912940,
                Version1_11_0_0 => 0x49461B0,
                Version1_12_0_0 => 0x4949340,
                Version1_13_0_0 => 0x494CB60,
                Version1_14_0_0 or Version1_15_0_0 => 0x494ECC0,
                Version1_15_1_0 => 0x49647B0,
                Version1_15_2_0 => 0x49647A0,
                _ => 0
            };

            CSDlc.Base = moduleBase + Version switch
            {
                Version1_8_0_0 => 0x4749DA0,
                Version1_9_0_0 or Version1_10_0_0 => 0x4749EE0,
                Version1_11_0_0 => 0x477D4A0,
                Version1_12_0_0 => 0x4780620,
                Version1_13_0_0 => 0x4783E40,
                Version1_14_0_0 or Version1_15_0_0 => 0x4785FA0,
                Version1_15_1_0 => 0x479B880,
                Version1_15_2_0 => 0x479B870,
                _ => 0
            };


            GameSpeed = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x980318,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x980218,
                Version1_5_0_0 => 0x980D08,
                Version1_5_1_0 => 0x980B38,
                Version1_6_0_0 => 0x981108,
                Version1_7_0_0 => 0x982018,
                Version1_8_0_0 => 0x98E568,
                Version1_9_0_0 => 0x98EB28,
                Version1_10_0_0 => 0x98EB98,
                Version1_11_0_0 => 0x997468,
                Version1_12_0_0 => 0x997EB8,
                Version1_13_0_0 => 0x999858,
                Version1_14_0_0 => 0x999B28,
                Version1_15_0_0 => 0x999C28,
                Version1_15_1_0 => 0x9A3D48,
                Version1_15_2_0 => 0x9A3E78,
                _ => 0
            };

            DrawNavigationPath = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4695290,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x46965B0,
                Version1_5_0_0 => 0x469A6B0,
                Version1_5_1_0 => 0x46996B0,
                Version1_6_0_0 => 0x469A710,
                Version1_7_0_0 => 0x469EF50,
                Version1_8_0_0 => 0x46FD6B0,
                Version1_9_0_0 or Version1_10_0_0 => 0x46FD7F0,
                Version1_11_0_0 => 0x4730AB0,
                Version1_12_0_0 => 0x4733C30,
                Version1_13_0_0 => 0x4737430,
                Version1_14_0_0 or Version1_15_0_0 => 0x4739590,
                Version1_15_1_0 => 0x47506D0,
                Version1_15_2_0 => 0x47506D0,
                _ => 0
            };


            Hooks.LastLockedTarget = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x847CFA,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x847A4A,
                Version1_5_0_0 => 0x84809A,
                Version1_5_1_0 => 0x847ECA,
                Version1_6_0_0 => 0x84849A,
                Version1_7_0_0 => 0x8493AA,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x852B7A,
                Version1_11_0_0 => 0x85857A,
                Version1_12_0_0 => 0x858D6A,
                Version1_13_0_0 => 0x85A61A,
                Version1_14_0_0 => 0x85A70A,
                Version1_15_0_0 => 0x85A74A,
                Version1_15_1_0 => 0x862CBA,
                Version1_15_2_0 => 0x86306A,
                _ => 0
            };


            Hooks.WarpCoords = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x61A84A,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x61AD7A,
                Version1_5_0_0 => 0x61B19A,
                Version1_5_1_0 => 0x61AFCA,
                Version1_6_0_0 => 0x61B59A,
                Version1_7_0_0 => 0x61C4AA,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x62292A,
                Version1_11_0_0 => 0x625F7A,
                Version1_12_0_0 => 0x6261FA,
                Version1_13_0_0 => 0x6262FA,
                Version1_14_0_0 or Version1_15_0_0 => 0x62632A,
                Version1_15_1_0 => 0x62874A,
                Version1_15_2_0 => 0x62873A,
                _ => 0
            };

            Hooks.WarpAngle = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x61A82A,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x61AD5A,
                Version1_5_0_0 => 0x61B17A,
                Version1_5_1_0 => 0x61AFAA,
                Version1_6_0_0 => 0x61B57A,
                Version1_7_0_0 => 0x61C48A,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x62290A,
                Version1_11_0_0 => 0x625F5A,
                Version1_12_0_0 => 0x6261DA,
                Version1_13_0_0 => 0x6262DA,
                Version1_14_0_0 or Version1_15_0_0 => 0x62630A,
                Version1_15_1_0 => 0x62872A,
                Version1_15_2_0 => 0x62871A,
                _ => 0
            };

            Hooks.AddSubGoal = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x3FC320,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x3FC3B0,
                Version1_5_0_0 or Version1_5_1_0 => 0x3FC410,
                Version1_6_0_0 => 0x3FC9E0,
                Version1_7_0_0 => 0x3FD8F0,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x3FDEF0,
                Version1_11_0_0 or Version1_12_0_0 => 0x3FDA80,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x3FDB10,
                Version1_15_1_0 or Version1_15_2_0 => 0x3FDB90,
                _ => 0
            };

            Hooks.InAirTimer = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x9B6A2A,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9B692A,
                Version1_5_0_0 => 0x9B741A,
                Version1_5_1_0 => 0x9B724A,
                Version1_6_0_0 => 0x9B781A,
                Version1_7_0_0 => 0x9B872A,
                Version1_8_0_0 => 0x9C4E3A,
                Version1_9_0_0 => 0x9C53FA,
                Version1_10_0_0 => 0x9C546A,
                Version1_11_0_0 => 0x9CF05A,
                Version1_12_0_0 => 0x9CFAAA,
                Version1_13_0_0 => 0x9D144A,
                Version1_14_0_0 => 0x9D171A,
                Version1_15_0_0 => 0x9D181A,
                Version1_15_1_0 => 0x9DB90A,
                Version1_15_2_0 => 0x9DBA3A,
                _ => 0
            };

            Hooks.NoClipKeyboard = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x17882E4,
                Version1_4_1_0 => 0x17889C4,
                Version1_4_2_0 or Version1_4_3_0 => 0x1788A14,
                Version1_5_0_0 => 0x178B154,
                Version1_5_1_0 => 0x178AF44,
                Version1_6_0_0 => 0x178B514,
                Version1_7_0_0 => 0x178DDB4,
                Version1_8_0_0 => 0x1870784,
                Version1_9_0_0 => 0x1870C04,
                Version1_10_0_0 => 0x1870C74,
                Version1_11_0_0 => 0x188AAD4,
                Version1_12_0_0 => 0x188BD14,
                Version1_13_0_0 => 0x188DFF4,
                Version1_14_0_0 => 0x188EBB4,
                Version1_15_0_0 => 0x188ECC4,
                Version1_15_1_0 => 0x189CB64,
                Version1_15_2_0 => 0x189CC94,
                _ => 0
            };

            Hooks.NoClipTriggers = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x1784980,
                Version1_4_1_0 => 0x1785060,
                Version1_4_2_0 or Version1_4_3_0 => 0x17850B0,
                Version1_5_0_0 => 0x17877F0,
                Version1_5_1_0 => 0x17875E0,
                Version1_6_0_0 => 0x1787BB0,
                Version1_7_0_0 => 0x178A450,
                Version1_8_0_0 => 0x186CE20,
                Version1_9_0_0 => 0x186D2A0,
                Version1_10_0_0 => 0x186D310,
                Version1_11_0_0 => 0x1887170,
                Version1_12_0_0 => 0x18883B0,
                Version1_13_0_0 => 0x188A690,
                Version1_14_0_0 => 0x188B250,
                Version1_15_0_0 => 0x188B360,
                Version1_15_1_0 => 0x1899200,
                Version1_15_2_0 => 0x1899330,
                _ => 0
            };

            Hooks.NoClipUpdateCoords = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x9B7570,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x9B7470,
                Version1_5_0_0 => 0x9B7F60,
                Version1_5_1_0 => 0x9B7D90,
                Version1_6_0_0 => 0x9B8360,
                Version1_7_0_0 => 0x9B9270,
                Version1_8_0_0 => 0x9C5980,
                Version1_9_0_0 => 0x9C5F40,
                Version1_10_0_0 => 0x9C5FB0,
                Version1_11_0_0 => 0x9CFBA0,
                Version1_12_0_0 => 0x9D05F0,
                Version1_13_0_0 => 0x9D1F90,
                Version1_14_0_0 => 0x9D2260,
                Version1_15_0_0 => 0x9D2360,
                Version1_15_1_0 => 0x9DC450,
                Version1_15_2_0 => 0x9DC580,
                _ => 0
            };

            Hooks.CameraUpLimit = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x50F30B,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x50F39B,
                Version1_5_0_0 => 0x50F67B,
                Version1_5_1_0 => 0x50F5CB,
                Version1_6_0_0 => 0x50FB9B,
                Version1_7_0_0 => 0x510AAB,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x513CFB,
                Version1_11_0_0 => 0x5147CB,
                Version1_12_0_0 => 0x51492B,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x514A4B,
                Version1_15_1_0 => 0x515ADB,
                Version1_15_2_0 => 0x515ACB,
                _ => 0
            };


            Hooks.ItemLotBase = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xDD4EC9,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xDD54D9,
                Version1_5_0_0 => 0xDD7C59,
                Version1_5_1_0 => 0xDD7A49,
                Version1_6_0_0 => 0xDD8019,
                Version1_7_0_0 => 0xDDA759,
                Version1_8_0_0 => 0xE06C59,
                Version1_9_0_0 => 0xE07229,
                Version1_10_0_0 => 0xE07299,
                Version1_11_0_0 => 0xE20579,
                Version1_12_0_0 => 0xE21629,
                Version1_13_0_0 => 0xE23689,
                Version1_14_0_0 => 0xE24119,
                Version1_15_0_0 => 0xE24229,
                Version1_15_1_0 => 0xE31049,
                Version1_15_2_0 => 0xE31179,
                _ => 0
            };

            Hooks.AddSubGoalDsa = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x3FC7D0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x3FC860,
                Version1_5_0_0 or Version1_5_1_0 => 0x3FC8C0,
                Version1_6_0_0 => 0x3FCE90,
                Version1_7_0_0 => 0x3FDDA0,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x3FE3A0,
                Version1_11_0_0 or Version1_12_0_0 => 0x3FDF30,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x3FDFC0,
                Version1_15_1_0 or Version1_15_2_0 => 0x3FE040,
                _ => 0
            };


            Hooks.SoulmassStaggerRemoval = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x98C40A,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x98C30A,
                Version1_5_0_0 => 0x98CDFA,
                Version1_5_1_0 => 0x98CC2A,
                Version1_6_0_0 => 0x98D1FA,
                Version1_7_0_0 => 0x98E10A,
                Version1_8_0_0 => 0x99A6FA,
                Version1_9_0_0 => 0x99ACBA,
                Version1_10_0_0 => 0x99AD2A,
                Version1_11_0_0 => 0x9A450A,
                Version1_12_0_0 => 0x9A4F5A,
                Version1_13_0_0 => 0x9A68FA,
                Version1_14_0_0 => 0x9A6BCA,
                Version1_15_0_0 => 0x9A6CCA,
                Version1_15_1_0 => 0x9B0DEA,
                Version1_15_2_0 => 0x9B0F1A,
                _ => 0
            };


            Hooks.LoadScreenItemName = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xB30E5D,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xB30F2D,
                Version1_5_0_0 => 0xB31D6D,
                Version1_5_1_0 => 0xB31B9D,
                Version1_6_0_0 => 0xB3216D,
                Version1_7_0_0 => 0xB337FD,
                Version1_8_0_0 => 0xB4894D,
                Version1_9_0_0 => 0xB48F0D,
                Version1_10_0_0 => 0xB48F7D,
                Version1_11_0_0 => 0xB5769D,
                Version1_12_0_0 => 0xB5811D,
                Version1_13_0_0 => 0xB59CCD,
                Version1_14_0_0 => 0xB59F9D,
                Version1_15_0_0 => 0xB5A09D,
                Version1_15_1_0 => 0xB6464D,
                Version1_15_2_0 => 0xB6477D,
                _ => 0
            };

            Hooks.DisableAllExceptTarget = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x5B8D20,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x5B8ED0,
                Version1_5_0_0 => 0x5B92B0,
                Version1_5_1_0 => 0x5B90E0,
                Version1_6_0_0 => 0x5B96B0,
                Version1_7_0_0 => 0x5BA5C0,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x5C08F0,
                Version1_11_0_0 => 0x5C3870,
                Version1_12_0_0 => 0x5C39E0,
                Version1_13_0_0 => 0x5C3B50,
                Version1_14_0_0 or Version1_15_0_0 => 0x5C3AF0,
                Version1_15_1_0 => 0x5C5E80,
                Version1_15_2_0 => 0x5C5E70,
                _ => 0
            };

            Patches.NoLogo = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xBBAFDF,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xBBB0CF,
                Version1_5_0_0 => 0xBBBF2F,
                Version1_5_1_0 => 0xBBBD5F,
                Version1_6_0_0 => 0xBBC32F,
                Version1_7_0_0 => 0xBBEA5F,
                Version1_8_0_0 => 0xBD6ACF,
                Version1_9_0_0 => 0xBD708F,
                Version1_10_0_0 => 0xBD70FF,
                Version1_11_0_0 => 0xBE6F8F,
                Version1_12_0_0 => 0xBE7D9F,
                Version1_13_0_0 => 0xBE993F,
                Version1_14_0_0 => 0xBE9C0F,
                Version1_15_0_0 => 0xBE9D0F,
                Version1_15_1_0 => 0xBF42BF,
                Version1_15_2_0 => 0xBF43EF,
                _ => 0
            };

            Patches.RepeatAct = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x3E0DA7,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x3E0E37,
                Version1_5_0_0 or Version1_5_1_0 => 0x3E0E97,
                Version1_6_0_0 => 0x3E1467,
                Version1_7_0_0 => 0x3E2377,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x3E28F7,
                Version1_11_0_0 or Version1_12_0_0 => 0x3E2487,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x3E2517,
                Version1_15_1_0 => 0x3E2597,
                Version1_15_2_0 => 0x3E2597,
                _ => 0
            };


            Patches.InfiniteDurability = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x5808B9,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x580A69,
                Version1_5_0_0 => 0x580D69,
                Version1_5_1_0 => 0x580B99,
                Version1_6_0_0 => 0x581169,
                Version1_7_0_0 => 0x582079,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x5880B9,
                Version1_11_0_0 => 0x58A1D9,
                Version1_12_0_0 => 0x58A339,
                Version1_13_0_0 => 0x58A489,
                Version1_14_0_0 or Version1_15_0_0 => 0x58A4C9,
                Version1_15_1_0 => 0x58BF69,
                Version1_15_2_0 => 0x58BF59,
                _ => 0
            };


            Patches.PlayerSoundView = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x412F96,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x413026,
                Version1_5_0_0 or Version1_5_1_0 => 0x413086,
                Version1_6_0_0 => 0x413656,
                Version1_7_0_0 => 0x414566,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x414B96,
                Version1_11_0_0 or Version1_12_0_0 => 0x414726,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4147B6,
                Version1_15_1_0 => 0x414836,
                Version1_15_2_0 => 0x414836,
                _ => 0
            };

            Patches.DebugFont = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x2313336,
                Version1_4_1_0 => 0x2313A16,
                Version1_4_2_0 or Version1_4_3_0 => 0x2313A66,
                Version1_5_0_0 => 0x2316066,
                Version1_5_1_0 => 0x2315E56,
                Version1_6_0_0 => 0x2316426,
                Version1_7_0_0 => 0x2318876,
                Version1_8_0_0 => 0x234AEA6,
                Version1_9_0_0 => 0x234B326,
                Version1_10_0_0 => 0x234B396,
                Version1_11_0_0 => 0x2369866,
                Version1_12_0_0 => 0x236B166,
                Version1_13_0_0 => 0x236D3A6,
                Version1_14_0_0 => 0x236DF66,
                Version1_15_0_0 => 0x236E076,
                Version1_15_1_0 => 0x237C736,
                Version1_15_2_0 => 0x237C866,
                _ => 0
            };


            Patches.NoRoll = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x5D6804,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x5D69B4,
                Version1_5_0_0 => 0x5D6D94,
                Version1_5_1_0 => 0x5D6BC4,
                Version1_6_0_0 => 0x5D7194,
                Version1_7_0_0 => 0x5D80A4,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x5DE484,
                Version1_11_0_0 => 0x5E1404,
                Version1_12_0_0 => 0x5E1644,
                Version1_13_0_0 => 0x5E1744,
                Version1_14_0_0 or Version1_15_0_0 => 0x5E1774,
                Version1_15_1_0 => 0x5E3AF4,
                Version1_15_2_0 => 0x5E3AE4,
                _ => 0
            };


            Patches.DbgDrawFlag = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x8BD8EB,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x8BD7DB,
                Version1_5_0_0 => 0x8BE2AB,
                Version1_5_1_0 => 0x8BE0DB,
                Version1_6_0_0 => 0x8BE6AB,
                Version1_7_0_0 => 0x8BF5BB,
                Version1_8_0_0 => 0x8CAA4B,
                Version1_9_0_0 => 0x8CAB0B,
                Version1_10_0_0 => 0x8CAB6B,
                Version1_11_0_0 => 0x8D15DB,
                Version1_12_0_0 => 0x8D1FBB,
                Version1_13_0_0 => 0x8D393B,
                Version1_14_0_0 => 0x8D3A2B,
                Version1_15_0_0 => 0x8D3A7B,
                Version1_15_1_0 => 0x8DC40B,
                Version1_15_2_0 => 0x8DC74B,
                _ => 0
            };


            Patches.IsWorldPaused = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x8D3989,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x8D3889,
                Version1_5_0_0 => 0x8D4359,
                Version1_5_1_0 => 0x8D4189,
                Version1_6_0_0 => 0x8D4759,
                Version1_7_0_0 => 0x8D5669,
                Version1_8_0_0 => 0x8E12F9,
                Version1_9_0_0 => 0x8E13B9,
                Version1_10_0_0 => 0x8E1419,
                Version1_11_0_0 => 0x8E810B,
                Version1_12_0_0 => 0x8E8AEB,
                Version1_13_0_0 => 0x8EA47B,
                Version1_14_0_0 => 0x8EA56B,
                Version1_15_0_0 => 0x8EA5BB,
                Version1_15_1_0 => 0x8F2F3B,
                Version1_15_2_0 => 0x8F306B,
                _ => 0
            };


            Patches.AccessFullShop = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xBE4094,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xBE4184,
                Version1_5_0_0 => 0xBE4F24,
                Version1_5_1_0 => 0xBE4D54,
                Version1_6_0_0 => 0xBE5324,
                Version1_7_0_0 => 0xBE7A54,
                Version1_8_0_0 => 0xBFFEB4,
                Version1_9_0_0 => 0xC00474,
                Version1_10_0_0 => 0xC004E4,
                Version1_11_0_0 => 0xC10524,
                Version1_12_0_0 => 0xC11334,
                Version1_13_0_0 => 0xC12ED4,
                Version1_14_0_0 => 0xC131A4,
                Version1_15_0_0 => 0xC132A4,
                Version1_15_1_0 => 0xC1D854,
                Version1_15_2_0 => 0xC1D984,
                _ => 0
            };


            Patches.DefaultSoundVolWrite = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x593DC6,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x593F76,
                Version1_5_0_0 => 0x594356,
                Version1_5_1_0 => 0x594186,
                Version1_6_0_0 => 0x594756,
                Version1_7_0_0 => 0x595666,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x59B806,
                Version1_11_0_0 => 0x59DF86,
                Version1_12_0_0 => 0x59E0E6,
                Version1_13_0_0 => 0x59E236,
                Version1_14_0_0 or Version1_15_0_0 => 0x59E276,
                Version1_15_1_0 => 0x5A0376,
                Version1_15_2_0 => 0x5A0366,
                _ => 0
            };

            Patches.StartMenuMusicPatch = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xAAAD71,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0xAAADB1,
                Version1_5_0_0 => 0xAABA21,
                Version1_5_1_0 => 0xAAB851,
                Version1_6_0_0 => 0xAABE21,
                Version1_7_0_0 => 0xAACE91,
                Version1_8_0_0 => 0xABD711,
                Version1_9_0_0 => 0xABDCD1,
                Version1_10_0_0 => 0xABDD41,
                Version1_11_0_0 => 0xACB7AD,
                Version1_12_0_0 => 0xACC1FD,
                Version1_13_0_0 => 0xACDDAD,
                Version1_14_0_0 => 0xACE07D,
                Version1_15_0_0 => 0xACE17D,
                Version1_15_1_0 => 0xAD897D,
                Version1_15_2_0 => 0xAD8AAD,
                _ => 0
            };


            Functions.BonfireWarp = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x482F10,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x482FA0,
                Version1_5_0_0 or Version1_5_1_0 => 0x483020,
                Version1_6_0_0 => 0x4835F0,
                Version1_7_0_0 => 0x484500,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x485FF0,
                Version1_11_0_0 => 0x486790,
                Version1_12_0_0 => 0x4867D0,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4868A0,
                Version1_15_1_0 or Version1_15_2_0 => 0x486A20,
                _ => 0
            };

            Functions.ItemSpawn = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x7AB590,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x7ABC00,
                Version1_5_0_0 => 0x7AC1E0,
                Version1_5_1_0 => 0x7AC010,
                Version1_6_0_0 => 0x7AC5E0,
                Version1_7_0_0 => 0x7AD4F0,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x7B6230,
                Version1_11_0_0 => 0x7BAFC0,
                Version1_12_0_0 => 0x7BB750,
                Version1_13_0_0 => 0x7BB940,
                Version1_14_0_0 => 0x7BBA30,
                Version1_15_0_0 => 0x7BBA70,
                Version1_15_1_0 => 0x7C3CD0,
                Version1_15_2_0 => 0x7C4080,
                _ => 0
            };


            Functions.SetEvent = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4BFB80,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4BFC10,
                Version1_5_0_0 or Version1_5_1_0 => 0x4BFEF0,
                Version1_6_0_0 => 0x4C04C0,
                Version1_7_0_0 => 0x4C13D0,
                Version1_8_0_0 => 0x4C43D0,
                Version1_9_0_0 or Version1_10_0_0 => 0x4C43E0,
                Version1_11_0_0 => 0x4C4DE0,
                Version1_12_0_0 => 0x4C4F40,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4C5060,
                Version1_15_1_0 => 0x4C5E30,
                Version1_15_2_0 => 0x4C5E20,
                _ => 0
            };

            Functions.GetEvent = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4BF0B0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4BF140,
                Version1_5_0_0 or Version1_5_1_0 => 0x4BF420,
                Version1_6_0_0 => 0x4BF9F0,
                Version1_7_0_0 => 0x4C0900,
                Version1_8_0_0 => 0x4C3900,
                Version1_9_0_0 or Version1_10_0_0 => 0x4C3910,
                Version1_11_0_0 => 0x4C4310,
                Version1_12_0_0 => 0x4C4470,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4C4590,
                Version1_15_1_0 => 0x4C5360,
                Version1_15_2_0 => 0x4C5350,
                _ => 0
            };

            Functions.BreakAllObjects = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x6699C0,
                Version1_4_1_0 => 0x669F90,
                Version1_4_2_0 => 0x66A0E3,
                Version1_4_3_0 => 0x669EA3,
                Version1_5_0_0 => 0x66A420,
                Version1_5_1_0 => 0x66A18C,
                Version1_6_0_0 => 0x66A800,
                Version1_7_0_0 => 0x66B727,
                Version1_8_0_0 => 0x671F20,
                Version1_9_0_0 => 0x671C57,
                Version1_10_0_0 => 0x671CBF,
                Version1_11_0_0 => 0x6756EA,
                Version1_12_0_0 => 0x675C23,
                Version1_13_0_0 => 0x675DF4,
                Version1_14_0_0 => 0x675D51,
                Version1_15_0_0 => 0x675E40,
                Version1_15_1_0 => 0x67826D,
                Version1_15_2_0 => 0x678450,
                _ => 0
            };

            Functions.RestoreAllObjects = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x669A40,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x66A010,
                Version1_5_0_0 => 0x66A480,
                Version1_5_1_0 => 0x66A2B0,
                Version1_6_0_0 => 0x66A880,
                Version1_7_0_0 => 0x66B790,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x671FA0,
                Version1_11_0_0 => 0x675720,
                Version1_12_0_0 => 0x675D80,
                Version1_13_0_0 => 0x675E80,
                Version1_14_0_0 or Version1_15_0_0 => 0x675EC0,
                Version1_15_1_0 => 0x6784E0,
                Version1_15_2_0 => 0x6784D0,
                _ => 0
            };

            Functions.ApplySpEffect = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x869980,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x8696D0,
                Version1_5_0_0 => 0x869D30,
                Version1_5_1_0 => 0x869B60,
                Version1_6_0_0 => 0x86A130,
                Version1_7_0_0 => 0x86B040,
                Version1_8_0_0 or Version1_9_0_0 => 0x874B30,
                Version1_10_0_0 => 0x874B20,
                Version1_11_0_0 => 0x87AA10,
                Version1_12_0_0 => 0x87B200,
                Version1_13_0_0 => 0x87CAC0,
                Version1_14_0_0 => 0x87CBB0,
                Version1_15_0_0 => 0x87CBF0,
                Version1_15_1_0 => 0x885270,
                Version1_15_2_0 => 0x885620,
                _ => 0
            };

            Functions.HasSpEffectId = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x86FAF0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x86F840,
                Version1_5_0_0 => 0x86FEA0,
                Version1_5_1_0 => 0x86FCD0,
                Version1_6_0_0 => 0x8702A0,
                Version1_7_0_0 => 0x8711B0,
                Version1_8_0_0 or Version1_9_0_0 => 0x87ADA0,
                Version1_10_0_0 => 0x87AD90,
                Version1_11_0_0 => 0x880FE0,
                Version1_12_0_0 => 0x8817D0,
                Version1_13_0_0 => 0x883090,
                Version1_14_0_0 => 0x883180,
                Version1_15_0_0 => 0x8831C0,
                Version1_15_1_0 => 0x88B850,
                Version1_15_2_0 => 0x88BC00,
                _ => 0
            };

            Functions.StopMusic = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x75ACB0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x75B310,
                Version1_5_0_0 => 0x75B820,
                Version1_5_1_0 => 0x75B650,
                Version1_6_0_0 => 0x75BC20,
                Version1_7_0_0 => 0x75CB30,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x7645D0,
                Version1_11_0_0 => 0x768860,
                Version1_12_0_0 => 0x768EC0,
                Version1_13_0_0 => 0x768FE0,
                Version1_14_0_0 or Version1_15_0_0 => 0x769020,
                Version1_15_1_0 => 0x76BA00,
                Version1_15_2_0 => 0x76B9F0,
                _ => 0
            };


            Functions.Rest = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x8A0890,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x8A06F0,
                Version1_5_0_0 => 0x8A10A0,
                Version1_5_1_0 => 0x8A0ED0,
                Version1_6_0_0 => 0x8A14A0,
                Version1_7_0_0 => 0x8A23B0,
                Version1_8_0_0 or Version1_9_0_0 => 0x8ACBB0,
                Version1_10_0_0 => 0x8ACC10,
                Version1_11_0_0 => 0x8B32D0,
                Version1_12_0_0 => 0x8B3B50,
                Version1_13_0_0 => 0x8B54C0,
                Version1_14_0_0 => 0x8B55B0,
                Version1_15_0_0 => 0x8B55F0,
                Version1_15_1_0 => 0x8BDFB0,
                Version1_15_2_0 => 0x8BE2A0,
                _ => 0
            };


            Functions.ChrInsByEntityId = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x4BC560,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4BC5F0,
                Version1_5_0_0 or Version1_5_1_0 => 0x4BC8D0,
                Version1_6_0_0 => 0x4BCEA0,
                Version1_7_0_0 => 0x4BDDB0,
                Version1_8_0_0 => 0x4C0DD0,
                Version1_9_0_0 or Version1_10_0_0 => 0x4C0DE0,
                Version1_11_0_0 => 0x4C17B0,
                Version1_12_0_0 => 0x4C1910,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4C1A30,
                Version1_15_1_0 => 0x4C2840,
                Version1_15_2_0 => 0x4C2830,
                _ => 0
            };

            Functions.SpawnSfxSimple = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x7B2DF0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x7B3490,
                Version1_5_0_0 => 0x7B3A70,
                Version1_5_1_0 => 0x7B38A0,
                Version1_6_0_0 => 0x7B3E70,
                Version1_7_0_0 => 0x7B4D80,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x7BDAC0,
                Version1_11_0_0 => 0x7C2A30,
                Version1_12_0_0 => 0x7C31C0,
                Version1_13_0_0 => 0x7C33B0,
                Version1_14_0_0 => 0x7C34A0,
                Version1_15_0_0 => 0x7C34E0,
                Version1_15_1_0 => 0x7CB470,
                Version1_15_2_0 => 0x7CB820,
                _ => 0
            };

            Functions.DestroyFxInner = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x19709B0,
                Version1_4_1_0 => 0x1971090,
                Version1_4_2_0 or Version1_4_3_0 => 0x19710E0,
                Version1_5_0_0 => 0x1973820,
                Version1_5_1_0 => 0x1973610,
                Version1_6_0_0 => 0x1973BE0,
                Version1_7_0_0 => 0x1976480,
                Version1_8_0_0 => 0x1A59220,
                Version1_9_0_0 => 0x1A596A0,
                Version1_10_0_0 => 0x1A59710,
                Version1_11_0_0 => 0x1A757A0,
                Version1_12_0_0 => 0x1A769E0,
                Version1_13_0_0 => 0x1A78CC0,
                Version1_14_0_0 => 0x1A79880,
                Version1_15_0_0 => 0x1A79990,
                Version1_15_1_0 => 0x1A878C0,
                Version1_15_2_0 => 0x1A879F0,
                _ => 0
            };

            Functions.RegisterPrismStoneSfx = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x95F7F0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x95F6F0,
                Version1_5_0_0 => 0x9601C0,
                Version1_5_1_0 => 0x95FFF0,
                Version1_6_0_0 => 0x9605C0,
                Version1_7_0_0 => 0x9614D0,
                Version1_8_0_0 => 0x96DA60,
                Version1_9_0_0 => 0x96DB20,
                Version1_10_0_0 => 0x96DB80,
                Version1_11_0_0 => 0x975930,
                Version1_12_0_0 => 0x976330,
                Version1_13_0_0 => 0x977CD0,
                Version1_14_0_0 => 0x977F80,
                Version1_15_0_0 => 0x977FD0,
                Version1_15_1_0 => 0x980840,
                Version1_15_2_0 => 0x980970,
                _ => 0
            };

            Functions.EzStateExternalEventTempCtor = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x1889440,
                Version1_4_1_0 => 0x1889B20,
                Version1_4_2_0 or Version1_4_3_0 => 0x1889B70,
                Version1_5_0_0 => 0x188C2B0,
                Version1_5_1_0 => 0x188C0A0,
                Version1_6_0_0 => 0x188C670,
                Version1_7_0_0 => 0x188EF10,
                Version1_8_0_0 => 0x1971CB0,
                Version1_9_0_0 => 0x1972130,
                Version1_10_0_0 => 0x19721A0,
                Version1_11_0_0 => 0x198DAA0,
                Version1_12_0_0 => 0x198ECE0,
                Version1_13_0_0 => 0x1990FC0,
                Version1_14_0_0 => 0x1991B80,
                Version1_15_0_0 => 0x1991C90,
                Version1_15_1_0 => 0x199FBC0,
                Version1_15_2_0 => 0x199FCF0,
                _ => 0
            };

            Functions.ExecuteTalkCommand = moduleBase + Version switch
            {
                Version1_3_2_0 => 0xE9EE30,
                Version1_4_1_0 => 0xE9F510,
                Version1_4_2_0 or Version1_4_3_0 => 0xE9F560,
                Version1_5_0_0 => 0xEA1CB0,
                Version1_5_1_0 => 0xEA1AA0,
                Version1_6_0_0 => 0xEA2070,
                Version1_7_0_0 => 0xEA4910,
                Version1_8_0_0 => 0xED6E50,
                Version1_9_0_0 => 0xED74F0,
                Version1_10_0_0 => 0xED7560,
                Version1_11_0_0 => 0xEF1400,
                Version1_12_0_0 => 0xEF2640,
                Version1_13_0_0 => 0xEF4920,
                Version1_14_0_0 => 0xEF54E0,
                Version1_15_0_0 => 0xEF55F0,
                Version1_15_1_0 => 0xF03490,
                Version1_15_2_0 => 0xF035C0,
                _ => 0
            };

            Functions.GetYMovement = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x7E4F80,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x7E5620,
                Version1_5_0_0 => 0x7E5C00,
                Version1_5_1_0 => 0x7E5A30,
                Version1_6_0_0 => 0x7E6000,
                Version1_7_0_0 => 0x7E6F10,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x7F0130,
                Version1_11_0_0 => 0x7F5280,
                Version1_12_0_0 => 0x7F5A70,
                Version1_13_0_0 => 0x7F5C60,
                Version1_14_0_0 => 0x7F5D50,
                Version1_15_0_0 => 0x7F5D90,
                Version1_15_1_0 => 0x7FDE70,
                Version1_15_2_0 => 0x7FE220,
                _ => 0
            };

            Functions.GetXMovement = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x7E5000,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x7E56A0,
                Version1_5_0_0 => 0x7E5C80,
                Version1_5_1_0 => 0x7E5AB0,
                Version1_6_0_0 => 0x7E6080,
                Version1_7_0_0 => 0x7E6F90,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x7F01B0,
                Version1_11_0_0 => 0x7F5300,
                Version1_12_0_0 => 0x7F5AF0,
                Version1_13_0_0 => 0x7F5CE0,
                Version1_14_0_0 => 0x7F5DD0,
                Version1_15_0_0 => 0x7F5E10,
                Version1_15_1_0 => 0x7FDEF0,
                Version1_15_2_0 => 0x7FE2A0,
                _ => 0
            };

            Functions.MatrixTransformVector = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x1257530,
                Version1_4_1_0 => 0x1257C10,
                Version1_4_2_0 or Version1_4_3_0 => 0x1257C60,
                Version1_5_0_0 => 0x125A3A0,
                Version1_5_1_0 => 0x125A190,
                Version1_6_0_0 => 0x125A760,
                Version1_7_0_0 => 0x125D000,
                Version1_8_0_0 => 0x1290760,
                Version1_9_0_0 => 0x1290E00,
                Version1_10_0_0 => 0x1290E70,
                Version1_11_0_0 => 0x12AACB0,
                Version1_12_0_0 => 0x12ABEF0,
                Version1_13_0_0 => 0x12AE1D0,
                Version1_14_0_0 => 0x12AED90,
                Version1_15_0_0 => 0x12AEEA0,
                Version1_15_1_0 => 0x12BCD40,
                Version1_15_2_0 => 0x12BCE70,
                _ => 0
            };

            Functions.GetPad = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x460260,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x4602F0,
                Version1_5_0_0 or Version1_5_1_0 => 0x460350,
                Version1_6_0_0 => 0x460920,
                Version1_7_0_0 => 0x461830,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x462170,
                Version1_11_0_0 or Version1_12_0_0 => 0x462560,
                Version1_13_0_0 or Version1_14_0_0 or Version1_15_0_0 => 0x4625F0,
                Version1_15_1_0 or Version1_15_2_0 => 0x462700,
                _ => 0
            };

            Functions.StartMenuMusic = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x75ABE0,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x75B240,
                Version1_5_0_0 => 0x75B750,
                Version1_5_1_0 => 0x75B580,
                Version1_6_0_0 => 0x75BB50,
                Version1_7_0_0 => 0x75CA60,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x764500,
                Version1_11_0_0 => 0x768790,
                Version1_12_0_0 => 0x768DF0,
                Version1_13_0_0 => 0x768F10,
                Version1_14_0_0 or Version1_15_0_0 => 0x768F50,
                Version1_15_1_0 => 0x76B930,
                Version1_15_2_0 => 0x76B920,
                _ => 0
            };

            Functions.FindAndRemoveSpEffect = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x86CA60,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x86C7B0,
                Version1_5_0_0 => 0x86CE10,
                Version1_5_1_0 => 0x86CC40,
                Version1_6_0_0 => 0x86D210,
                Version1_7_0_0 => 0x86E120,
                Version1_8_0_0 or Version1_9_0_0 => 0x877C30,
                Version1_10_0_0 => 0x877C20,
                Version1_11_0_0 => 0x87DCB0,
                Version1_12_0_0 => 0x87E4A0,
                Version1_13_0_0 => 0x87FD60,
                Version1_14_0_0 => 0x87FE50,
                Version1_15_0_0 => 0x87FE90,
                Version1_15_1_0 => 0x888520,
                Version1_15_2_0 => 0x8888D0,
                _ => 0
            };
            
            Functions.GetItemQuantity = moduleBase + Version switch
            {
                Version1_3_2_0 => 0x580980,
                Version1_4_1_0 or Version1_4_2_0 or Version1_4_3_0 => 0x580B30,
                Version1_5_0_0 => 0x580E30,
                Version1_5_1_0 => 0x580C60,
                Version1_6_0_0 => 0x581230,
                Version1_7_0_0 => 0x582140,
                Version1_8_0_0 or Version1_9_0_0 or Version1_10_0_0 => 0x588180,
                Version1_11_0_0 => 0x58A2A0,
                Version1_12_0_0 => 0x58A400,
                Version1_13_0_0 => 0x58A550,
                Version1_14_0_0 or Version1_15_0_0 => 0x58A590,
                Version1_15_1_0 => 0x58C030,
                Version1_15_2_0 => 0x58C020,
                _ => 0
            };

        }

#if DEBUG
        public static void PrintAll()
        {
            Console.WriteLine("--- Bases ---");
            PrintOffset("WorldChrManImp", WorldChrManImp.Base);
            PrintOffset("GameMan", GameMan.Base);
            PrintOffset("DamageMan", DamageMan.Base);
            PrintOffset("EventFlagMan", EventFlagMan.Base);
            PrintOffset("GameDataMan", GameDataMan.Base);
            PrintOffset("DebugFlags", DebugFlags.Base);
            PrintOffset("SprjDbgEvent", SprjDbgEvent.Base);
            PrintOffset("SoloParamRepo", SoloParamRepo.Base);
            PrintOffset("MenuMan", MenuMan.Base);
            PrintOffset("LuaEventMan", LuaEventMan.Base);
            PrintOffset("AiTargetingFlags", AiTargetingFlags.Base);
            PrintOffset("MapItemMan", MapItemMan.Base);
            PrintOffset("HitFlags", HitFlags.Base);
            PrintOffset("WorldObjManImpl", WorldObjManImpl.Base);
            PrintOffset("FieldArea", FieldArea.Base);
            PrintOffset("GroupMask", GroupMask.Base);
            PrintOffset("UserInputManager", UserInputManager.Base);
            PrintOffset("SprjFlipper", SprjFlipper.Base);
            PrintOffset("SprjBulletManager", SprjBulletManager.Base);
            PrintOffset("FD4PadManager", FD4PadManager.Base);
            PrintOffset("CSDlc", CSDlc.Base);
            PrintOffset("GameSpeed", GameSpeed);
            PrintOffset("DrawNavigationPath", DrawNavigationPath);

            Console.WriteLine("\n--- Hooks ---");
            PrintOffset("LastLockedTarget", Hooks.LastLockedTarget);
            PrintOffset("WarpCoords", Hooks.WarpCoords);
            PrintOffset("WarpAngle", Hooks.WarpAngle);
            PrintOffset("AddSubGoal", Hooks.AddSubGoal);
            PrintOffset("InAirTimer", Hooks.InAirTimer);
            PrintOffset("NoClipKeyboard", Hooks.NoClipKeyboard);
            PrintOffset("NoClipTriggers", Hooks.NoClipTriggers);
            PrintOffset("NoClipUpdateCoords", Hooks.NoClipUpdateCoords);
            PrintOffset("ItemLotBase", Hooks.ItemLotBase);
            PrintOffset("LoadScreenItemName", Hooks.LoadScreenItemName);
            PrintOffset("CameraUpLimit", Hooks.CameraUpLimit);
            PrintOffset("AddSubGoalDsa", Hooks.AddSubGoalDsa);
            PrintOffset("SoulmassStaggerRemoval", Hooks.SoulmassStaggerRemoval);

            Console.WriteLine("\n--- Patches ---");
            PrintOffset("NoLogo", Patches.NoLogo);
            PrintOffset("AccessFullShop", Patches.AccessFullShop);
            PrintOffset("RepeatAct", Patches.RepeatAct);
            PrintOffset("InfiniteDurability", Patches.InfiniteDurability);
            PrintOffset("PlayerSoundView", Patches.PlayerSoundView);
            PrintOffset("DebugFont", Patches.DebugFont);
            PrintOffset("NoRoll", Patches.NoRoll);
            PrintOffset("DbgDrawFlag", Patches.DbgDrawFlag);
            PrintOffset("IsWorldPaused", Patches.IsWorldPaused);
            PrintOffset("DefaultSoundVolWrite", Patches.DefaultSoundVolWrite);
            PrintOffset("StartMenuMusicPatch", Patches.StartMenuMusicPatch);

            Console.WriteLine("\n--- Functions ---");
            PrintOffset("BonfireWarp", Functions.BonfireWarp);
            PrintOffset("SetEvent", Functions.SetEvent);
            PrintOffset("GetEvent", Functions.GetEvent);
            PrintOffset("ApplySpEffect", Functions.ApplySpEffect);
            PrintOffset("HasSpEffectId", Functions.HasSpEffectId);
            PrintOffset("Rest", Functions.Rest);
            PrintOffset("StopMusic", Functions.StopMusic);
            PrintOffset("ChrInsByEntityId", Functions.ChrInsByEntityId);
            PrintOffset("SpawnSfxSimple", Functions.SpawnSfxSimple);
            PrintOffset("DestroyFxInner", Functions.DestroyFxInner);
            PrintOffset("RegisterPrismStoneSfx", Functions.RegisterPrismStoneSfx);
            PrintOffset("EzStateExternalEventTempCtor", Functions.EzStateExternalEventTempCtor);
            PrintOffset("ExecuteTalkCommand", Functions.ExecuteTalkCommand);
            PrintOffset("BreakAllObjects", Functions.BreakAllObjects);
            PrintOffset("RestoreAllObjects", Functions.RestoreAllObjects);
            PrintOffset("ItemSpawn", Functions.ItemSpawn);
            PrintOffset("GetYMovement", Functions.GetYMovement);
            PrintOffset("GetXMovement", Functions.GetXMovement);
            PrintOffset("MatrixTransformVector", Functions.MatrixTransformVector);
            PrintOffset("GetPad", Functions.GetPad);
            PrintOffset("StartMenuMusic", Functions.StartMenuMusic);
            PrintOffset("FindAndRemoveSpEffect", Functions.FindAndRemoveSpEffect);
            PrintOffset("GetItemQuantity", Functions.GetItemQuantity);

            Console.WriteLine("\n====================================\n");
        }

        private static void PrintOffset(string name, nint value) => PrintOffset(name, (long)value);

        private static void PrintOffset(string name, long value)
        {
            var rel = value - _moduleBase;
            Console.WriteLine(rel <= 0
                ? $"  {name,-40} *** NOT SET ***"
                : $"  {name,-40} 0x{value:X}  (0x{rel:X})");
        }
#endif
    }
}
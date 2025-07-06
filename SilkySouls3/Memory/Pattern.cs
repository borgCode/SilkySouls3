﻿namespace SilkySouls3.Memory
{
    public class Pattern
    {
        public byte[] Bytes { get; }
        public string Mask { get; }
        public int InstructionOffset { get; }
        public AddressingMode AddressingMode { get; }
        public int OffsetLocation { get; }
        public int InstructionLength { get; }

        public Pattern(byte[] bytes, string mask, int instructionOffset, AddressingMode addressingMode,
            int offsetLocation = 0, int instructionLength = 0)
        {
            Bytes = bytes;
            Mask = mask;
            InstructionOffset = instructionOffset;
            AddressingMode = addressingMode;
            OffsetLocation = offsetLocation;
            InstructionLength = instructionLength;
        }
    }

    public enum AddressingMode
    {
        Absolute,
        Relative,
        Direct32
    }

    public static class Patterns
    {
        public static readonly Pattern WorldChrMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x85, 0xC9, 0x0F, 0x84, 0xAD, 0x00 },
            "xxx????xxxxxxx",
            0,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern GameMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0xC6, 0x80, 0x30 },
            "xxx????xxx",
            0,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern SoloParamRepo = new Pattern(
            new byte[] { 0x89, 0x11, 0x33, 0xDB, 0x48, 0x89, 0x59, 0x08, 0x85, 0xD2 },
            "xxxxxxxxxx",
            0x10,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern LuaEventMan = new Pattern(
            new byte[] { 0x0F, 0xB6, 0xF8, 0x48, 0x83, 0x3D },
            "xxxxxx",
            3,
            AddressingMode.Relative,
            3,
            8
        );

        public static readonly Pattern AiTargetingFlags = new Pattern(
            new byte[] { 0x81, 0xE2, 0xFF, 0x7F, 0xFD },
            "xxxxx",
            0x31,
            AddressingMode.Relative,
            2,
            6
        );

        public static readonly Pattern MenuMan = new Pattern(
            new byte[] { 0x48, 0x39, 0x81, 0x50 },
            "xxxx",
            -0x9,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern EventFlagMan = new Pattern(
            new byte[] { 0xBE, 0x02, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x0D },
            "xxxxxx????xxx",
            0xA,
            AddressingMode.Relative,
            3,
            7
        );


        public static readonly Pattern DebugFlags = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74, 0x0A, 0xC7 },
            "xx????xxxx",
            0,
            AddressingMode.Relative,
            2,
            7
        );

        public static readonly Pattern DebugEvent = new Pattern(
            new byte[] { 0x02, 0x00, 0x48, 0x83, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x75 },
            "xxxxx????xx",
            0x2,
            AddressingMode.Relative,
            3,
            8
        );

        public static readonly Pattern MapItemMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8D, 0x94, 0x24, 0xC8 },
            "xxx????xxxxx",
            0,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern GameDataMan = new Pattern(
            new byte[] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x80, 0xB8, 0x82 },
            "x????xxx",
            -0x2,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern DamageMan = new Pattern(
            new byte[] { 0x8B, 0x53, 0x2C, 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00 },
            "xxxxxx????",
            0x3,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern FieldArea = new Pattern(
            new byte[] { 0x48, 0x85, 0xC9, 0x74, 0x10, 0x48, 0x8B, 0x49 },
            "xxxxxxxx",
            -0x7,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern SprjFlipper = new Pattern(
            new byte[] { 0x45, 0x89, 0x58, 0x5C },
            "xxxx",
            0x4,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern WorldObjManImpl = new Pattern(
            new byte[] { 0x48, 0x83, 0xEC, 0x20, 0x83, 0x3A, 0xFF, 0x48, 0x8B, 0xDA, 0x75, 0x28 },
            "xxxxxxxxxxxx",
            0xE,
            AddressingMode.Relative,
            3,
            7
        );


        //Patch


        public static readonly Pattern DbgDrawFlag = new Pattern(
            new byte[] { 0x80, 0x78, 0x65, 0x00 },
            "xxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoLogo = new Pattern(
            new byte[]
            {
                0xE8, 0x00, 0x00, 0x00, 0x00, 0x90, 0x4D, 0x8B, 0xC7, 0x49, 0x8B, 0xD4, 0x48, 0x8B, 0xC8, 0xE8, 0x00,
                0x00, 0x00, 0x00, 0xC7, 0x44, 0x24, 0x50
            },
            "x????xxxxxxxxxxx????xxxx",
            0,
            AddressingMode.Absolute
        );


        public static readonly Pattern AccessFullShop = new Pattern(
            new byte[] { 0x84, 0xC0, 0x74, 0x14, 0x48, 0x8D, 0x54, 0x24, 0x38 },
            "xxxxxxxxx",
            0,
            AddressingMode.Absolute
        );


        public static readonly Pattern RepeatAct = new Pattern(
            new byte[] { 0x0F, 0xBE, 0x80, 0x81 },
            "xxxx",
            3,
            AddressingMode.Absolute
        );

        public static readonly Pattern GameSpeed = new Pattern(
            new byte[] { 0x00, 0x00, 0x80, 0x3F, 0xF3, 0x0F, 0x10, 0x8B },
            "xxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern InfiniteDurability = new Pattern(
            new byte[] { 0x0F, 0x85, 0x9C, 0x00, 0x00, 0x00, 0x8B, 0x87 },
            "xxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern PlayerSoundView = new Pattern(
            new byte[] { 0x80, 0x79, 0x28, 0x00, 0x48, 0x8B, 0xF2, 0x74 },
            "xxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern DebugFont = new Pattern(
            new byte[] { 0x7F, 0x4C, 0x24, 0x20, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x74 },
            "xxxxx????xxx",
            4,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoRoll = new Pattern(
            new byte[]
            {
                0xC6, 0x83, 0x10, 0x04, 0x00, 0x00, 0x01, 0xF3, 0x0F, 0x11, 0x83, 0x0C, 0x04, 0x00, 0x00, 0xC6
            },
            "xxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern FreeCamPatch = new Pattern(
            new byte[] { 0x80, 0xBB, 0x98, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x84 },
            "xxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern GroupMask = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x00 },
            "xx????xxx",
            0,
            AddressingMode.Relative,
            2,
            7
        );

        public static readonly Pattern UserInputManager = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5F, 0x38, 0x48, 0x89, 0x6F, 0x58
            },
            "xxxxxxxx",
            0x8,
            AddressingMode.Relative,
            3,
            7
        );

        public static readonly Pattern HitIns = new Pattern(
            new byte[] { 0x44, 0x0F, 0xB6, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x45 },
            "xxxx????x",
            0,
            AddressingMode.Relative,
            4,
            8
        );

        //Hooks

        public static readonly Pattern LockedTarget = new Pattern(
            new byte[] { 0x48, 0x8B, 0x80, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x08, 0x48, 0x8B, 0x51 },
            "xxx????xxxxxx",
            0,
            AddressingMode.Absolute);


        public static readonly Pattern WarpCoordWrite = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x40 },
            "xxx????xxxxxxxx",
            0xA,
            AddressingMode.Absolute
        );

        public static readonly Pattern CameraUpLimit = new Pattern(
            new byte[] { 0xF3, 0x0F, 0x11, 0x86, 0xFC, 0x01 },
            "xxxxxx",
            0,
            AddressingMode.Absolute
        );


        public static readonly Pattern ArgoSpeed = new Pattern(
            new byte[] { 0x48, 0x8D, 0x44, 0x24, 0x20, 0xF3, 0x0F, 0x10, 0x0B, 0x0F, 0x2F, 0xC8, 0xF3 },
            "xxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern AddSubGoal = new Pattern(
            new byte[]
            {
                0x48, 0x8B, 0xC4, 0x48, 0x81, 0xEC, 0x98, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8C, 0x24, 0xD0
            },
            "xxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoClipInAirTimer = new Pattern(
            new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01 },
            "xxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern ItemLotBase = new Pattern(
            new byte[] { 0x45, 0x0F, 0xB7, 0x41, 0x40, 0x41 },
            "xxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoClipKeyboard = new Pattern(
            new byte[] { 0x49, 0xC1, 0xE8, 0x05, 0x48, 0x8B, 0x93 },
            "xxxxxxx",
            -0x138,
            AddressingMode.Absolute
        );


        public static readonly Pattern NoClipTriggers = new Pattern(
            new byte[] { 0x0F, 0x2F, 0xFE, 0x72, 0x2D, 0x44, 0x0F, 0x2F, 0xC7, 0x72, 0x27 },
            "xxxxxxxxxxx",
            -0x9,
            AddressingMode.Absolute
        );

        public static readonly Pattern NoClipUpdateCoords = new Pattern(
            new byte[] { 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00, 0x0F },
            "xxxxxxxx",
            -0x1,
            AddressingMode.Absolute
        );


        //Funcs

        public static readonly Pattern WarpFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x18, 0x56, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF1, 0x48, 0x8B, 0x49
            },
            "xxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern GetEvent = new Pattern(
            new byte[] { 0xBA, 0xCE, 0x1F, 0x00, 0x00, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x84 },
            "xxxxxx????x",
            5,
            AddressingMode.Relative,
            1,
            5
        );


        public static readonly Pattern BreakAllObjects = new Pattern(
            new byte[] { 0x48, 0x83, 0xEC, 0x28, 0x8B, 0x81, 0xE8, 0x00, 0x02, 0x00, 0x85, 0xC0, 0x74, 0x6D },
            "xxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern RestoreAllObjects = new Pattern(
            new byte[] { 0x48, 0x83, 0xEC, 0x28, 0x8B, 0x81, 0xE8, 0x00, 0x02, 0x00, 0x85, 0xC0, 0x74, 0x62 },
            "xxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );


        public static readonly Pattern ItemSpawnFunc = new Pattern(
            new byte[] { 0x48, 0x8D, 0x55, 0xA4, 0x48, 0x8B, 0x4C, 0x24, 0x50, 0xE8, 0x00, 0x00, 0x00, 0x00, },
            "xxxxxxxxxx????",
            0x9,
            AddressingMode.Relative,
            1,
            5
        );

        public static readonly Pattern SetEvent = new Pattern(
            new byte[]
            {
                0x80, 0xB9, 0x28, 0x02, 0x00, 0x00, 0x00, 0x45, 0x0F, 0xB6, 0xF9
            },
            "xxxxxxxxxxx",
            -0xB,
            AddressingMode.Absolute);

        public static readonly Pattern TravelFunc = new Pattern(
            new byte[]
            {
                0x40, 0x55, 0x53, 0x56, 0x57, 0x41, 0x56, 0x48, 0x8D, 0x6C, 0x24, 0xC9, 0x48, 0x81, 0xEC, 0x00, 0x01,
                0x00, 0x00, 0x48, 0xC7, 0x45, 0x97
            },
            "xxxxxxxxxxxxxxxxxxxxxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern CombineMenuFlagAndEventFlag = new Pattern(
            new byte[] { 0x81, 0xF9, 0xF3, 0x01 },
            "xxxx",
            0,
            AddressingMode.Absolute
        );

        public static readonly Pattern SetSpEffect = new Pattern(
            new byte[] {
                0xC6, 0x44, 0x24, 0x20, 0x00, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x48, 0x81, 0xC4, 0x88, 0x00, 0x00, 0x00,
                0xC3 
            },
            "xxxxxx????xxxxxxxx",
            5,
            AddressingMode.Relative,
            1,
            5
        );
    }
}
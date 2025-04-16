using static SilkySouls3.Memory.RipType;

namespace SilkySouls3.Memory
{
    public class Pattern
    {
        public byte[] Bytes { get; }
        public string Mask { get; }
        public int InstructionOffset { get; }
        public RipType RipType { get; }

        public Pattern(byte[] bytes, string mask, int instructionOffset, RipType ripType)
        {
            Bytes = bytes;
            Mask = mask;
            InstructionOffset = instructionOffset;
            RipType = ripType;
        }
    }

    public static class Patterns
    {
        public static readonly Pattern WorldChrMan = new Pattern(
            new byte[] { 0x48, 0x39, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x74, 0x50 },
            "xxx????xx",
            0,
            Mov64
        );

        public static readonly Pattern GameMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x80, 0xB8, 0xC1 },
            "xxx????xxx",
            0,
            Mov64
        );

        public static readonly Pattern SoloParamRepo = new Pattern(
            new byte[] { 0x89, 0x11, 0x33, 0xDB, 0x48, 0x89, 0x59, 0x08, 0x85, 0xD2 },
            "xxxxxxxxxx",
            0x10,
            Mov64
        );

        public static readonly Pattern LuaEventMan = new Pattern(
            new byte[] { 0x0F, 0xB6, 0xF8, 0x48, 0x83, 0x3D },
            "xxxxxx",
            3,
            QwordCmp
        );


        //TODO Maybe not neceessary
        public static readonly Pattern WorldAiMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x89, 0x7C, 0x24, 0x30, 0x8B, 0x78, 0x10 },
            "xxx????xxxxxxxx",
            0,
            Mov64
        );

        public static readonly Pattern AiTargetingFlags = new Pattern(
            new byte[] { 0x81, 0xE2, 0xFF, 0x7F, 0xFD },
            "xxxxx",
            0x31,
            Mov32);

        public static readonly Pattern MenuMan = new Pattern(
            new byte[] { 0x48, 0x39, 0x81, 0x50 },
            "xxxx",
            -0x9,
            Mov64
        );

        public static readonly Pattern EventFlagMan = new Pattern(
            new byte[] { 0xBE, 0x02, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x0D },
            "xxxxxx????xxx",
            0xA,
            Mov64
        );


        public static readonly Pattern DebugFlags = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74, 0x0A, 0xC7 },
            "xx????xxxx",
            0,
            Cmp
        );

        public static readonly Pattern DebugEvent = new Pattern(
            new byte[] { 0x02, 0x00, 0x48, 0x83, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x75 },
            "xxxxx????xx",
            0x2,
            QwordCmp
        );

        public static readonly Pattern MapItemMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8D, 0x94, 0x24, 0xC8 },
            "xxx????xxxxx",
            0,
            Mov64
        );

        public static readonly Pattern GameDataMan = new Pattern(
            new byte[] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x80, 0xB8, 0x82 },
            "x????xxx",
            -0x2,
            Mov64
        );

        public static readonly Pattern DamageMan = new Pattern(
            new byte[] { 0x8B, 0x53, 0x2C, 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00 },
            "xxxxxx????",
            0x3,
            Mov64
        );

        public static readonly Pattern FieldArea = new Pattern(
            new byte[] { 0x48, 0x85, 0xC9, 0x74, 0x10, 0x48, 0x8B, 0x49 },
            "xxxxxxxx",
            -0x7,
            Mov64
        );


        //Patch


        public static readonly Pattern TargetingView = new Pattern(
            new byte[] { 0x80, 0x78, 0x65, 0x00 },
            "xxxx",
            0,
            None
        );
        
        public static readonly Pattern NoLogo = new Pattern(
            new byte[]
            {
                0xE8, 0x00, 0x00, 0x00, 0x00, 0x90, 0x4D, 0x8B, 0xC7, 0x49, 0x8B, 0xD4, 0x48, 0x8B, 0xC8, 0xE8, 0x00,
                0x00, 0x00, 0x00, 0xC7, 0x44, 0x24, 0x50
            },
            "x????xxxxxxxxxxx????xxxx",
            0,
            None
        );


        public static readonly Pattern RepeatAct = new Pattern(
            new byte[] { 0x0F, 0xBE, 0x80, 0x81 },
            "xxxx",
            3,
            None
        );

        public static readonly Pattern GameSpeed = new Pattern(
            new byte[] { 0x00, 0x00, 0x80, 0x3F, 0xF3, 0x0F, 0x10, 0x8B },
            "xxxxxxxx",
            0,
            None
        );

        public static readonly Pattern InfiniteDurability = new Pattern(
            new byte[] { 0x0F, 0x85, 0x9C, 0x00, 0x00, 0x00, 0x8B, 0x87 },
            "xxxxxxxx",
            0,
            None
        );

        public static readonly Pattern PlayerSoundView = new Pattern(
            new byte[] { 0x80, 0x79, 0x28, 0x00, 0x48, 0x8B, 0xF2, 0x74 },
            "xxxxxxxx",
            0,
            None
        );

        public static readonly Pattern DebugFont = new Pattern(
            new byte[] { 0x7F, 0x4C, 0x24, 0x20, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x74 },
            "xxxxx????xxx",
            4,
            None
        );

        public static readonly Pattern NoRoll = new Pattern(
            new byte[]
            {
                0xC6, 0x83, 0x10, 0x04, 0x00, 0x00, 0x01, 0xF3, 0x0F, 0x11, 0x83, 0x0C, 0x04, 0x00, 0x00, 0xC6
            },
            "xxxxxxxxxxxxxxxx",
            0,
            None
        );


        public static readonly Pattern GroupMask = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x00 },
            "xx????xxx",
            0,
            Cmp
        );

        public static readonly Pattern UserInputManager = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5F, 0x38, 0x48, 0x89, 0x6F, 0x58
            },
            "xxxxxxxx",
            0x8,
            Mov64
        );
        

        //Hooks

        public static readonly Pattern LockedTarget = new Pattern(
            new byte[] { 0x48, 0x8B, 0x80, 0x90, 0x1F, 0x00, 0x00, 0x48, 0x8B, 0x08, 0x48, 0x8B, 0x51 },
            "xxxxxxxxxxxxx",
            0,
            None);


        public static readonly Pattern WarpCoordWrite = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x40 },
            "xxx????xxxxxxxx",
            0xA,
            None
        );

        public static readonly Pattern AddSubGoal = new Pattern(
            new byte[]
            {
                0x48, 0x8B, 0xC4, 0x48, 0x81, 0xEC, 0x98, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x10, 0x8C, 0x24, 0xD0
            },
            "xxxxxxxxxxxxxxxx",
            0,
            None
        );

        public static readonly Pattern NoClipInAirTimer = new Pattern(
            new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01 },
            "xxxxxx",
            0,
            None
        );


        public static readonly Pattern NoClipKeyboard = new Pattern(
            new byte[] { 0x49, 0xC1, 0xE8, 0x05, 0x48, 0x8B, 0x93 },
            "xxxxxxx",
            -0x138,
            None
        );


        public static readonly Pattern NoClipTriggers = new Pattern(
            new byte[] { 0x0F, 0x2F, 0xFE, 0x72, 0x2D, 0x44, 0x0F, 0x2F, 0xC7, 0x72, 0x27 },
            "xxxxxxxxxxx",
            -0x9,
            None
        );

        public static readonly Pattern NoClipUpdateCoords = new Pattern(
            new byte[] { 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00, 0x0F },
            "xxxxxxxx",
            -0x1,
            None
        );


        //Funcs

        public static readonly Pattern WarpFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x18, 0x56, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF1, 0x48, 0x8B, 0x49
            },
            "xxxxxxxxxxxxxxxx",
            0,
            None
        );

        public static readonly Pattern ItemSpawnFunc = new Pattern(
            new byte[]
            {
                0x48, 0x8D, 0x6C, 0x24, 0xD9, 0x48, 0x81, 0xEC, 0x00, 0x01, 0x00, 0x00, 0x48, 0xC7, 0x45, 0xCF
            },
            "xxxxxxxxxxxxxxxx",
            -0x10,
            None
        );

        public static readonly Pattern SetEvent = new Pattern(
            new byte[]
            {
                0x80, 0xB9, 0x28, 0x02, 0x00, 0x00, 0x00, 0x45, 0x0F, 0xB6, 0xF9
            },
            "xxxxxxxxxxx",
            -0xB,
            None);

        public static readonly Pattern TravelFunc = new Pattern(
            new byte[]
            {
                0x40, 0x55, 0x53, 0x56, 0x57, 0x41, 0x56, 0x48, 0x8D, 0x6C, 0x24, 0xC9, 0x48, 0x81, 0xEC, 0x00, 0x01,
                0x00, 0x00, 0x48, 0xC7, 0x45, 0x97
            },
            "xxxxxxxxxxxxxxxxxxxxxxx",
            0,
            None
        );

        public static readonly Pattern CombineMenuFlagAndEventFlag = new Pattern(
            new byte[] { 0x81, 0xF9, 0xF3, 0x01 },
            "xxxx",
            0,
            None
        );
    }
}
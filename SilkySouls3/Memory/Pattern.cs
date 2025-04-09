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
            RipType.Mov64
        );

        public static readonly Pattern GameMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x80, 0xB8, 0xC1 },
            "xxx????xxx",
            0,
            RipType.Mov64
        );

        public static readonly Pattern DamageMan = new Pattern(
            new byte[] { 0xC7, 0x44, 0x24, 0x28, 0xFF, 0xFF, 0xFF, 0xFF, 0x48, 0x89, 0x7C, 0x24, 0x20, 0x0F },
            "xxxxxxxxxxxxxx",
            0x15,
            RipType.Mov64
        );


        public static readonly Pattern SoloParamRepo = new Pattern(
            new byte[] { 0x89, 0x11, 0x33, 0xDB, 0x48, 0x89, 0x59, 0x08, 0x85, 0xD2 },
            "xxxxxxxxxx",
            0x10,
            RipType.Mov64
        );
        
        public static readonly Pattern LuaEventMan = new Pattern(
            new byte[] { 0x0F, 0xB6, 0xF8, 0x48, 0x83, 0x3D },
            "xxxxxx",
            3,
            RipType.QwordCmp
        );



        //TODO Maybe not neceessary
        public static readonly Pattern WorldAiMan = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x89, 0x7C, 0x24, 0x30, 0x8B, 0x78, 0x10 },
            "xxx????xxxxxxxx",
            0,
            RipType.Mov64
        );

        public static readonly Pattern AiTargetingFlags = new Pattern(
            new byte[] { 0x81, 0xE2, 0xFF, 0x7F, 0xFD },
            "xxxxx",
            0x31,
            RipType.Mov32);

        public static readonly Pattern MenuMan = new Pattern(
            new byte[] { 0x48, 0x39, 0x81, 0x50 },
            "xxxx",
            -0x9,
            RipType.Mov64
        );

        public static readonly Pattern DebugFlags = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74, 0x0A, 0xC7 },
            "xx????xxxx",
            0,
            RipType.Cmp
        );

        public static readonly Pattern DebugEvent = new Pattern(
            new byte[] { 0x02, 0x00, 0x48, 0x83, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x75 },
            "xxxxx????xx",
            0x2,
            RipType.QwordCmp
        );

        
        //Patch

        public static readonly Pattern RepeatAct = new Pattern(
            new byte[] { 0x0F, 0xBE, 0x80, 0x81 },
            "xxxx",
            0,
            RipType.None
        );
            
        
        
        //Hooks

        public static readonly Pattern LockedTarget = new Pattern(
            new byte[] { 0x48, 0x8B, 0x80, 0x90, 0x1F, 0x00, 0x00, 0x48, 0x8B, 0x08, 0x48, 0x8B, 0x51 },
            "xxxxxxxxxxxxx",
            0,
            RipType.None);


        public static readonly Pattern WarpCoordWrite = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x28, 0x01, 0x66, 0x0F, 0x7F, 0x80, 0x40 },
            "xxx????xxxxxxxx",
            0,
            RipType.None
        );

        public static readonly Pattern AddSubGoal = new Pattern(
            new byte[] { 0x48, 0x83, 0xC4, 0x68, 0xC3, 0xCC, 0x81 },
            "xxxxxxx",
            -0xA,
            RipType.None
        );
        
     // PLayer coords for noclip   // public static readonly Pattern Placeholder = new Pattern(
        //     new byte[] { 0x48, 0x8B, 0x48, 0x18, 0x8D, 0x46 },
        //     "xxxxxx",


        
        //Funcs

        public static readonly Pattern WarpFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x18, 0x56, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF1, 0x48, 0x8B, 0x49
            },
            "xxxxxxxxxxxxxxxx",
            0,
            RipType.None
        );

    }
}
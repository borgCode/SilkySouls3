namespace SilkySouls3.Data
{
    public static class ArgoData
    {
        public const int TalkParamSpeedOffset = 0x2C;

        public static readonly (uint RowId, float VanillaDuration)[] TalkRows =
        [
            (88000200, 10f),
            (88000201, 8f),
            (88000202, 9f),
            (88000203, 6f),
            (88000204, 3f),
            (88000600, 4f),
            (88000601, 9f)
        ];
    }
}

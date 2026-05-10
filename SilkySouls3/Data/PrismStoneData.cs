using System;

namespace SilkySouls3.Data
{
    public static class PrismStoneData
    {
        public static readonly (float X, float Y, float Z)[] Points =
        {
            (-378.1000f, -55.3100f, -281.8500f),
            (-377.5936f, -55.3100f, -281.2373f),
            (-377.1380f, -55.3100f, -280.5880f),
            (-376.6544f, -55.3100f, -279.9588f),
            (-376.1760f, -55.3100f, -279.3260f),
            (-375.7240f, -55.3100f, -278.6740f),
            (-375.2456f, -55.3100f, -278.0412f),
            (-374.7620f, -55.3100f, -277.4120f),
            (-374.3064f, -55.3100f, -276.7627f),
            (-373.8000f, -55.3100f, -276.1500f),
            (-380.0000f, -55.3100f, -279.4500f),
            (-379.6625f, -55.3100f, -278.7374f),
            (-379.3353f, -55.3100f, -278.0206f),
            (-379.0167f, -55.3100f, -277.3001f),
            (-378.6706f, -55.3100f, -276.5911f),
            (-378.3416f, -55.3100f, -275.8749f),
            (-378.0058f, -55.3100f, -275.1617f),
            (-377.6942f, -55.3100f, -274.4383f),
            (-377.3411f, -55.3100f, -273.7322f),
            (-377.0208f, -55.3100f, -273.0125f),
            (-376.6764f, -55.3100f, -272.3028f),
            (-376.3647f, -55.3100f, -271.5794f),
            (-376.0117f, -55.3100f, -270.8733f),
            (-375.7000f, -55.3100f, -270.1500f),
        };

        public static readonly byte[] TransformBytes = BuildTransformBytes();

        private static byte[] BuildTransformBytes()
        {
            var buf = new byte[Points.Length * 0x20];

            for (var i = 0; i < Points.Length; i++)
            {
                var off = i * 0x20;
                var (x, y, z) = Points[i];

                BitConverter.GetBytes(x).CopyTo(buf, off + 0x00);
                BitConverter.GetBytes(y).CopyTo(buf, off + 0x04);
                BitConverter.GetBytes(z).CopyTo(buf, off + 0x08);
                BitConverter.GetBytes(1.0f).CopyTo(buf, off + 0x1C);
            }

            return buf;
        }
    }
}

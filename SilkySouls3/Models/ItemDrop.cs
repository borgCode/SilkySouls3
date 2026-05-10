// 

using System.Numerics;

namespace SilkySouls3.Models;

public class ItemDrop(Vector3 coords, float angle, uint flags, uint requestType, uint itemKey, uint quantity)
{
    public Vector3 Coords { get; set; } = coords;
    public float Angle { get; set; } = angle;
    public uint Flags { get; set; } = flags;
    public uint RequestType { get; set; } = requestType;
    public uint ItemKey { get; set; } = itemKey;
    public uint Quantity { get; set; } = quantity;
}

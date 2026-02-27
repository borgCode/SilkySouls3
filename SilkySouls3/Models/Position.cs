// 

using System.Numerics;

namespace SilkySouls3.Models;

public class Position(int blockId, Vector3 coords, float angle)
{
    public uint BlockId;
    public int CeremonyId = -1;
    public Vector3 Coords;
    public float Angle;
}
//

using System.Numerics;

namespace SilkySouls3.Models;

public class Position
{
    public uint BlockId;
    public int CeremonyId = -1;
    public Vector3 Coords;
    public float Angle;

    public Position() { }

    public Position(uint blockId, Vector3 coords, float angle)
    {
        BlockId = blockId;
        Coords = coords;
        Angle = angle;
    }
}

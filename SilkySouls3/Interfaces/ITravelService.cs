//

using System.Collections.Generic;
using System.Numerics;
using SilkySouls3.Enums;
using SilkySouls3.Models;

namespace SilkySouls3.Interfaces;

public interface ITravelService
{
    void Warp(int bonfireId);
    void WarpWithCoords(Vector3 coords, float angle, int bonfireId);
    void UnlockBonfires(IEnumerable<WarpLocation> warps);
    bool TryResolveBonfire(uint blockId, int ceremonyId, out int bonfireId, out DlcRequirement dlcRequirement);
}
//

using System.Collections.Generic;
using SilkySouls3.Models;

namespace SilkySouls3.Interfaces;

public interface ISpEffectService
{
    void ApplySpEffect(nint chrIns, uint spEffectId);
    void RemoveSpEffect(nint chrIns, uint spEffectId);
    bool HasSpEffect(nint chrIns, uint spEffectId);
    List<SpEffectEntry> GetActiveSpEffectList(nint chrIns);
}
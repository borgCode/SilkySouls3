//

using System.Collections.Generic;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class SpEffectService(IMemoryService memoryService, IReminderService reminderService) : ISpEffectService
{
    private const int SpEffectEntrySize = 0x80;

    public void ApplySpEffect(nint chrIns, uint spEffectId)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ApplySpEffect);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (chrIns, 2),
            (Functions.ApplySpEffect, 0x12 + 2)
        ]);

        AsmHelper.WriteImmediateDword(bytes, (int)spEffectId, 0xA + 1);
        memoryService.AllocateAndExecute(bytes);
    }

    public void RemoveSpEffect(nint chrIns, uint spEffectId)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.FindAndRemoveSpEffect);
        AsmHelper.WriteAbsoluteAddresses(bytes, new[]
        {
            (chrIns, 2),
            (Functions.FindAndRemoveSpEffect, 0x13 + 2)
        });
        
        AsmHelper.WriteImmediateDword(bytes, (int)spEffectId, 0xA + 1);
        
        memoryService.AllocateAndExecute(bytes);
    }

    public bool HasSpEffect(nint chrIns, uint spEffectId)
    {
        var specialEffect = memoryService.Read<nint>(chrIns + ChrIns.SpecialEffect);
        var current = memoryService.Read<nint>(specialEffect + ChrIns.SpecialEffectOffsets.Head);
        
        while (current != 0)
        {
            if (memoryService.Read<uint>(current + ChrIns.SpEffectEntry.Id) == spEffectId) return true;
            current = memoryService.Read<nint>(current + ChrIns.SpEffectEntry.Next);
        }
        return false;
    }

    public List<SpEffectEntry> GetActiveSpEffectList(nint chrIns)
    {
        reminderService.TrySetReminder();
        var spEffectList = new List<SpEffectEntry>();
        var specialEffect = memoryService.Read<nint>(chrIns + ChrIns.SpecialEffect);
        var current = memoryService.Read<nint>(specialEffect + ChrIns.SpecialEffectOffsets.Head);

        while (current != 0)
        {
            var entry = new MemoryBlock(memoryService.ReadBytes(current, SpEffectEntrySize));
            
            int id = entry.Get<int>(ChrIns.SpEffectEntry.Id);
            float timeLeft = entry.Get<float>(ChrIns.SpEffectEntry.TimeLeft);
            float duration = entry.Get<float>(ChrIns.SpEffectEntry.Duration);
            nint paramData = entry.Get<nint>(ChrIns.SpEffectEntry.SpEffectParam);
            nint next = entry.Get<nint>(ChrIns.SpEffectEntry.Next);
            
            ushort stateInfo = memoryService.Read<ushort>(paramData + ChrIns.SpEffectParamData.StateInfo);
            
            spEffectList.Add(new SpEffectEntry(id, timeLeft, duration, stateInfo));
            current = next;
        }

        return spEffectList;
    }
}
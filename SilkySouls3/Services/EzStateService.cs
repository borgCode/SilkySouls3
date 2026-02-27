// 

using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class EzStateService(IMemoryService memoryService) : IEzStateService
{
    public void ExecuteTalkCommand(EzState.TalkCommand command)
    {
        if (command == null) return;
        
        var code = CustomCodeOffsets.Base + CustomCodeOffsets.EzStateTalkCode;
        var paramsLoc = CustomCodeOffsets.Base + CustomCodeOffsets.EzStateTalkParams;
        
        for (int i = 0; i < command.Params.Length; i++)
        {
            memoryService.Write(paramsLoc + i * 4, command.Params[i]);
        }
        
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ExecuteTalkEvent);
        AsmHelper.WriteRelativeOffsets(bytes, [
        (code + 0x16, Functions.EzStateExternalEventTempCtor, 5, 0x16 + 1),
        (code + 0x51, paramsLoc, 7, 0x51 + 3),
        (code + 0x91, Functions.ExecuteTalkCommand, 5, 0x91 + 1)
        ]);
        
        
        AsmHelper.WriteImmediateDwords(bytes, [
            (command.CommandId, 0x11 + 1),
            (command.Params.Length, 0x4A + 1)
        ]);
        
        memoryService.WriteBytes(code, bytes);
        memoryService.RunThread(code);
    }
}
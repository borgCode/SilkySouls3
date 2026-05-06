using System.Collections.Generic;
using System.Linq;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class EventService(IMemoryService memoryService) : IEventService
    {
        public void SetEvent(int eventId, bool setVal)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.SetEvent);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
                (memoryService.Read<nint>(EventFlagMan.Base), 2),
                (Functions.SetEvent, 0x1f + 2)
            ]);

            AsmHelper.WriteImmediateDwords(bytes, [
                (eventId, 0xA + 1),
                (setVal ? 1 : 0, 0xF + 2)
            ]);

            memoryService.AllocateAndExecute(bytes);
        }

        public void BatchSetEvent(IReadOnlyList<int> eventIds, bool setVal)
        {
            const int eventCountSize = 4;
            const int codeSize = 0x50;
            const int eventIdSize = 4;
            var count = eventIds.Count;
            var allocatedMem = memoryService.AllocateMem(
                (uint)(codeSize + eventCountSize + count * eventIdSize));

            var eventsStart = allocatedMem + 0x4;
            var code = eventsStart + eventIds.Count * eventIdSize;
            
            
            var bytes = AsmLoader.GetAsmBytes(AsmScript.BatchSetEvent);
            
            AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x18, eventsStart, 7, 0x18 + 3),
            (code + 0x21, allocatedMem, 6, 0x21 + 2),
            ]);
            
            AsmHelper.WriteAbsoluteAddresses(bytes, [
            (memoryService.Read<nint>(EventFlagMan.Base), 0x4 + 2),
            (Functions.SetEvent, 0xE + 2)
            ]);
            
            AsmHelper.WriteImmediateDword(bytes, setVal ? 1 : 0, 0x31 + 2);
            
            memoryService.Write(allocatedMem, count);
            
            var idsArray = eventIds as int[] ?? eventIds.ToArray();
            memoryService.WriteArray<int>(eventsStart, idsArray);
            
            memoryService.WriteBytes(code, bytes);
            
            memoryService.RunThread(code);
            memoryService.FreeMem(allocatedMem);
        }

        public bool GetEvent(int eventId)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.GetEvent);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
                (memoryService.Read<nint>(EventFlagMan.Base), 2),
                (eventId, 0xA + 2),
                (Functions.GetEvent, 0x17 + 2),
                (CustomCodeOffsets.Base + CustomCodeOffsets.GetEventResult, 0x2B + 2)
            ]);

            memoryService.AllocateAndExecute(bytes);
            return memoryService.Read<byte>(CustomCodeOffsets.Base + CustomCodeOffsets.GetEventResult) == 1;
        }

        public void ToggleDisableEvents(bool isEnabled) => memoryService.Write(
            memoryService.Read<nint>(SprjDbgEvent.Base) + SprjDbgEvent.DisableEvent, isEnabled);

        public void ToggleDrawEvents(bool isEnabled) =>
            memoryService.Write(memoryService.Read<nint>(SprjDbgEvent.Base) + SprjDbgEvent.EventDraw, isEnabled);
    }
}
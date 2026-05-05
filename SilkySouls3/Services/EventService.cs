using System.Collections.Generic;
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

        public void BatchSetEvent(IEnumerable<int> eventIds, bool setVal)
        {
            foreach (var id in eventIds)
                SetEvent(id, setVal);
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
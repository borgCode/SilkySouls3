using System;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class EventService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        
        public EventService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void SetEvent(ulong flagId, int setVal)
        {
            var eventMan = _memoryIo.ReadInt64(EventFlagMan.Base);
            var setEventBytes = AsmLoader.GetAsmBytes("SetEvent");
            var bytes = BitConverter.GetBytes(eventMan);
            Array.Copy(bytes, 0, setEventBytes, 0x2, 8);
            bytes = BitConverter.GetBytes(flagId);
            Array.Copy(bytes, 0, setEventBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(setVal);
            Array.Copy(bytes, 0, setEventBytes, 0x14 + 2, 4);
            bytes = BitConverter.GetBytes(Funcs.SetEvent);
            Array.Copy(bytes, 0, setEventBytes, 0x24 + 2, 8);
            _memoryIo.AllocateAndExecute(setEventBytes);
        }

        public void SetMultipleEventsOn(params ulong[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId, 1);
            }
        }

        public bool GetEvent(ulong flagId)
        {
            var bytes = AsmLoader.GetAsmBytes("GetEvent");
            AsmHelper.WriteAbsoluteAddresses(bytes, new []
            {
                (_memoryIo.ReadInt64(EventFlagMan.Base), 0x0 + 2),
                ((long)flagId, 0xA + 2),
                (Funcs.GetEvent, 0x17 + 2),
                (CodeCaveOffsets.Base.ToInt64() + CodeCaveOffsets.GetEventResult, 0x2B + 2)
            });

            _memoryIo.AllocateAndExecute(bytes);
            return _memoryIo.ReadUInt8(CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult) == 1;
        }
        
        public void ToggleDisableEvent(bool isDisableEventEnabled)
        {
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(DebugEvent.Base) + DebugEvent.DisableEvent,
                isDisableEventEnabled ? 1 : 0);
        }

        public void ToggleArgoHook(bool isArgoSpeedEnabled)
        {
            var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ArgoSpeed.Code;
            if (isArgoSpeedEnabled)
            {
                var speedLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ArgoSpeed.Speed;
                _memoryIo.WriteFloat(speedLoc, 2.0f);
                var hookLoc = Hooks.ArgoSpeed;
                var bytes = AsmLoader.GetAsmBytes("ArgoSpeed");
                AsmHelper.WriteRelativeOffsets(bytes, new []
                {
                    (code.ToInt64() + 0xA, speedLoc.ToInt64(), 6, 0xA + 2),
                    (code.ToInt64() + 0x1A, hookLoc + 0x5, 5, 0x1A + 1)
                });
                
                _memoryIo.WriteBytes(code, bytes);
                _hookManager.InstallHook(code.ToInt64(), hookLoc, new byte[]
                    { 0x48, 0x8D, 0x44, 0x24, 0x20 });
            }
            else
            {
                _hookManager.UninstallHook(code.ToInt64());
            }
        }

        public void SetArgoSpeed(float value)
        {
            var speedLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ArgoSpeed.Speed;
            _memoryIo.WriteFloat(speedLoc, value);
        }
    }
}
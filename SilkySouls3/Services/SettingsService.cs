using System;
using SilkySouls3.Memory;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class SettingsService
    {
        private readonly MemoryIo _memoryIo;
        
        public SettingsService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }
        
        public void Quitout() =>
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(MenuMan.Base) + (int)MenuMan.MenuManOffsets.QuitOut, 1);
        
        public void ToggleStutterFix(bool isEnabled) => 
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(UserInputManager.Base) + UserInputManager.SteamInputEnum,
                isEnabled? 1 : 0);
    }
}
using System;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class SettingsService
    {
        private readonly MemoryIo _memoryIo;
        private readonly NopManager _nopManager;
        
        public SettingsService(MemoryIo memoryIo, NopManager nopManager)
        {
            _memoryIo = memoryIo;
            _nopManager = nopManager;
        }
        
        public void Quitout() =>
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(MenuMan.Base) + (int)MenuMan.MenuManOffsets.QuitOut, 1);
        
        public void ToggleStutterFix(bool isEnabled) => 
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(UserInputManager.Base) + UserInputManager.SteamInputEnum,
                isEnabled? 1 : 0);

        public void PatchDefaultSound(int defaultSoundVolume)
        {
            var defaultSoundWrite = Patches.DefaultSoundVolWrite;
            _memoryIo.WriteByte(defaultSoundWrite + 0x4, defaultSoundVolume);
            _memoryIo.WriteByte(defaultSoundWrite + 0x5, defaultSoundVolume);
        }

        public void ToggleDisableMusic(bool isDisableMenuMusicEnabled)
        {
            if (isDisableMenuMusicEnabled)
            {
                _nopManager.InstallNop(Patches.StartMenuMusic.ToInt64(), 5);

                var bytes = AsmLoader.GetAsmBytes("StopMusic");
                var funcBytes = BitConverter.GetBytes(Funcs.StopMusic);
                Array.Copy(funcBytes, 0, bytes, 0x4 + 2, 8 );
                _memoryIo.AllocateAndExecute(bytes);
            }
            else
            {
                _nopManager.RestoreNop(Patches.StartMenuMusic.ToInt64());
            }
        }
    }
}
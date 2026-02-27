using System;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class SettingsService(IMemoryService memoryService) : ISettingsService
    {
        public void Quitout() =>
            memoryService.Write(memoryService.Read<nint>(MenuMan.Base) + MenuMan.Quitout, true);

        public void ToggleStutterFix(bool isEnabled) =>
            memoryService.Write(memoryService.Read<nint>(UserInputManager.Base) + UserInputManager.SteamInputEnum,
                isEnabled);

        public void PatchDefaultSound(int defaultSoundVolume)
        {
            var defaultSoundWrite = Patches.DefaultSoundVolWrite;
            memoryService.Write(defaultSoundWrite + 0x4, (byte)defaultSoundVolume);
            memoryService.Write(defaultSoundWrite + 0x5, (byte)defaultSoundVolume);
        }

        public void ToggleDisableMusic(bool isDisableMenuMusicEnabled)
        {
            if (isDisableMenuMusicEnabled)
            {
                memoryService.WriteBytes(Patches.StartMenuMusicPatch, [0x90, 0x90, 0x90, 0x90, 0x90]);

                var bytes = AsmLoader.GetAsmBytes(AsmScript.StopMusic);
                var funcBytes = BitConverter.GetBytes(Functions.StopMusic);
                Array.Copy(funcBytes, 0, bytes, 0x4 + 2, 8);
                memoryService.AllocateAndExecute(bytes);
            }
            else
            {
                var rel = (int)(Functions.StartMenuMusic - (Patches.StartMenuMusicPatch + 5));
                memoryService.WriteBytes(Patches.StartMenuMusicPatch, [0xE8, ..BitConverter.GetBytes(rel)]);
            }
        }
    }
}
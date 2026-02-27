// 

namespace SilkySouls3.Interfaces;

public interface ISettingsService
{
    void Quitout();
    void ToggleStutterFix(bool isEnabled);
    void PatchDefaultSound(int defaultSoundVolume);
    void ToggleDisableMusic(bool isDisableMenuMusicEnabled);
}
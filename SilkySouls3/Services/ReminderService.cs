// 

using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class ReminderService : IReminderService
{
    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;

    private const string ReminderText = "SS3 Active";
    
    private bool _hasDoneReminder;

    public ReminderService(IMemoryService memoryService, HookManager hookManager, IStateService stateService)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;
        stateService.Subscribe(State.Attached, OnAttached);
    }

    private void OnAttached() => _hasDoneReminder = false;

    public void TrySetReminder()
    {
        if (_hasDoneReminder) return;

        var textLoc = CustomCodeOffsets.Base + CustomCodeOffsets.ReminderText;
        _memoryService.WriteWString(textLoc, ReminderText);

        var code = CustomCodeOffsets.Base + CustomCodeOffsets.SetReminderCode;
        var bytes = AsmLoader.GetAsmBytes(AsmScript.TrySetReminder);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, textLoc, 7, 0x7 + 3),
            (code + 0xE, Hooks.LoadScreenItemName + 7, 5, 0xE + 1)
        ]);
        
        _memoryService.WriteBytes(code, bytes);
        _hookManager.InstallHook(code, Hooks.LoadScreenItemName, [0x48, 0x8D, 0x8B, 0x28, 0x0B, 0x00, 0x00]);
    }
}
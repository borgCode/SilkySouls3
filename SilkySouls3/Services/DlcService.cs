// 

using SilkySouls3.Interfaces;
using SilkySouls3.Enums;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class DlcService(IMemoryService memoryService) : IDlcService
{
    public bool MeetsRequirement(DlcRequirement requirement)
    {
        return requirement switch
        {
            DlcRequirement.Always => true,
            DlcRequirement.Dlc1 => IsDlc1Available,
            DlcRequirement.Dlc2 => IsDlc2Available,
            _ => false
        };
    }

    public void CheckDlc()
    {
        var csDlc = memoryService.Read<nint>(CSDlc.Base);
        if (csDlc == 0)
        {
            IsDlc1Available = false;
            IsDlc2Available = false;
            return;
        }
        IsDlc1Available = memoryService.Read<byte>(csDlc + CSDlc.Dlc1) == 1;
        IsDlc2Available = memoryService.Read<byte>(csDlc + CSDlc.Dlc2) == 1;
    }

    public bool IsDlc1Available { get; private set; }
    public bool IsDlc2Available { get; private set; }
}

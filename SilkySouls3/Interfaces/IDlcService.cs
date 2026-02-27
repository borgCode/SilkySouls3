// 

using SilkySouls3.Enums;

namespace SilkySouls3.Interfaces;

public interface IDlcService
{
    void CheckDlc();
    public bool IsDlc1Available { get; }
    public bool IsDlc2Available { get; }
    public bool MeetsRequirement(DlcRequirement requirement);
}

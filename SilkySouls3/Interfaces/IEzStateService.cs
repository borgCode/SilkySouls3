// 

using SilkySouls3.GameIds;

namespace SilkySouls3.Interfaces;

public interface IEzStateService
{
    void ExecuteTalkCommand(EzState.TalkCommand command);
}
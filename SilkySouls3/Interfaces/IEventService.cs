// 

namespace SilkySouls3.Interfaces;

public interface IEventService
{
    void SetEvent(int eventId, bool setVal);
    bool GetEvent(int eventId);
    void ToggleDisableEvents(bool isEnabled);
    void ToggleDrawEvents(bool isEnabled);
}
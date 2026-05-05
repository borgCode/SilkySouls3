// 

using System.Collections.Generic;

namespace SilkySouls3.Interfaces;

public interface IEventService
{
    void SetEvent(int eventId, bool setVal);
    void BatchSetEvent(IEnumerable<int> eventIds, bool setVal);
    bool GetEvent(int eventId);
    void ToggleDisableEvents(bool isEnabled);
    void ToggleDrawEvents(bool isEnabled);
}
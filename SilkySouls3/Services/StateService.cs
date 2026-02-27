// 

using System;
using System.Collections.Generic;
using System.Linq;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;

namespace SilkySouls3.Services;

public class StateService(IMemoryService memoryService) : IStateService
{
    private readonly Dictionary<State, List<Action>> _eventHandlers = new();
    private readonly Dictionary<State, List<Delegate>> _valueEventHandlers = new();

    public bool IsLoaded()
    {
        var worldChrMan = memoryService.Read<nint>(Offsets.WorldChrManImp.Base);
        return memoryService.Read<nint>(worldChrMan + Offsets.WorldChrManImp.PlayerIns) != 0;
    }

    public void Publish(State eventType)
    {
        if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
        {
            foreach (var handler in eventHandler)
                handler.Invoke();
        }
    }
    
    public void Publish<T>(State eventType, T value)
    {
        if (_valueEventHandlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers.OfType<Action<T>>())
                handler.Invoke(value);
        }
    }

    public void Subscribe(State eventType, Action handler)
    {
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = new List<Action>();

        _eventHandlers[eventType].Add(handler);
    }
    
    public void Subscribe<T>(State eventType, Action<T> handler)
    {
        if (!_valueEventHandlers.ContainsKey(eventType))
            _valueEventHandlers[eventType] = new List<Delegate>();

        _valueEventHandlers[eventType].Add(handler);
    }


    public void Unsubscribe(State eventType, Action handler)
    {
        if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
            eventHandler.Remove(handler);
    }
    
    public void Unsubscribe<T>(State eventType, Action<T> handler)
    {
        if (_valueEventHandlers.TryGetValue(eventType, out var handlers))
            handlers.Remove(handler);
    }
}
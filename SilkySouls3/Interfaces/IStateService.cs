// 

using System;
using SilkySouls3.Enums;

namespace SilkySouls3.Interfaces;

public interface IStateService
{
    public bool IsLoaded();
    void Publish(State eventType);
    void Publish<T>(State eventType, T value);
    void Subscribe(State eventType, Action handler);
    void Subscribe<T>(State eventType, Action<T> handler);
    void Unsubscribe(State eventType, Action handler);
    void Unsubscribe<T>(State eventType, Action<T> handler);
}
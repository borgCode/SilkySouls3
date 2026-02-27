// 

using System;

namespace SilkySouls3.Interfaces;

public interface IGameTickService
{
    public void Subscribe(Action callback);
    public void Unsubscribe(Action callback);
}
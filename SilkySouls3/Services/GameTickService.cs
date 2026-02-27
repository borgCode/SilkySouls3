// 

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;

namespace SilkySouls3.Services;

public class GameTickService : IGameTickService
{
    private readonly DispatcherTimer _timer;
    private readonly List<Action> _subscribers = new();
    
    public GameTickService(IStateService stateService)
    {
        stateService.Subscribe(State.Loaded, OnGameLoaded);
        stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(64) };
        _timer.Tick += OnTick;
    }
    
    public void Subscribe(Action callback)
    {
        if (!_subscribers.Contains(callback))
            _subscribers.Add(callback);
    }
    public void Unsubscribe(Action callback) => _subscribers.Remove(callback);
    

    private void OnGameLoaded() => _timer.Start();
    
    private void OnGameNotLoaded() => _timer.Stop();

    private void OnTick(object sender, EventArgs e)
    {
        foreach (var subscriber in _subscribers)
            subscriber();
    }
}
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class DebugDrawService : IDebugDrawService
    {
        private readonly IMemoryService _memoryService;
        private int _clientCount;

        public DebugDrawService(IMemoryService memoryService, IStateService stateService)
        {
            _memoryService = memoryService;
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
        }

        public void RequestDebugDraw()
        {
            if (_clientCount == 0) TogglePatch(true);
            
            _clientCount++;
        }

        private void TogglePatch(bool isEnabled) => _memoryService.Write(Patches.DbgDrawFlag + 0x3, isEnabled);

        public void ReleaseDebugDraw()
        {
            _clientCount--;

            if (_clientCount == 0) TogglePatch(false);

            if (_clientCount < 0)
                _clientCount = 0;
        }

        private void OnGameNotLoaded()
        {
            _clientCount = 0;
            TogglePatch(false);
        }
    }
}
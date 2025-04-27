using SilkySouls3.Memory;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class DebugDrawService
    {
        private int _clientCount;
        private readonly MemoryIo _memoryIo;

        public DebugDrawService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public void RequestDebugDraw()
        {
            if (_clientCount == 0) TogglePatch(true);
            
            _clientCount++;
        }

        private void TogglePatch(bool isEnabled) => _memoryIo.WriteByte(Patches.DbgDrawFlag + 0x3, isEnabled ? 1 : 0);

        public void ReleaseDebugDraw()
        {
            _clientCount--;

            if (_clientCount == 0) TogglePatch(false);

            if (_clientCount < 0)
                _clientCount = 0;
        }

        public void Reset()
        {
            _clientCount = 0;
            TogglePatch(false);
        }
    }
}
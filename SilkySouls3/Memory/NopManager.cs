using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkySouls3.Memory
{
    public class NopManager
    {
        private readonly MemoryIo _memoryIo;
        private readonly Dictionary<long, byte[]> _nopRegistry = new Dictionary<long, byte[]>();

        public NopManager(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }


        public void InstallNop(long address, int length)
        {
            if (_nopRegistry.ContainsKey(address))
                return;
            byte[] originalBytes = _memoryIo.ReadBytes((IntPtr)address, length);
            byte[] nopBytes = Enumerable.Repeat((byte)0x90, length).ToArray();
        
            _memoryIo.WriteBytes((IntPtr)address, nopBytes);
            _nopRegistry[address] = originalBytes;
        }
    
        public void RestoreNop(long address)
        {
            if (_nopRegistry.TryGetValue(address, out byte[] originalBytes))
            {
                _memoryIo.WriteBytes((IntPtr)address, originalBytes);
                _nopRegistry.Remove(address);
            }
        }

        public void ClearRegistry() => _nopRegistry.Clear();
        
        
        public void RestoreAll()
        {
            foreach (var key in _nopRegistry.Keys.ToList())
            {
                RestoreNop(key);
            }
        }
    }
}
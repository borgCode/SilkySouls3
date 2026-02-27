using System;
using SilkySouls3.Interfaces;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class ParamService(IMemoryService memoryService) : IParamService
    {
        public nint GetParamRow(int tableIndex, uint rowId)
        {
            var data = GetParamData(tableIndex);
            if (data is not var (paramData, rowCount, descriptorBase)) return IntPtr.Zero;

            int low = 0, high = rowCount - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                var entry = descriptorBase + mid * 0x8;

                var id = memoryService.Read<uint>(entry);

                if (id == rowId)
                {
                    var rowIndex = memoryService.Read<int>(entry + 0x4);
                    if (rowIndex < 0) return IntPtr.Zero;

                    var dataOffset = memoryService.Read<nint>(paramData + (rowIndex + 3) * 0x18);
                    return paramData + dataOffset;
                }

                if (id < rowId)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return IntPtr.Zero;
        }

        public void PrintAllParamTableNames()
        {
            var soloParamRepo = memoryService.Read<nint>(SoloParamRepo.Base);

            for (int tableIndex = 0; tableIndex < 0x61; tableIndex++)
            {
                var tableBase = soloParamRepo + tableIndex * 0x48;

                var capacity = memoryService.Read<int>(tableBase + 0x68);
                if (capacity <= 0) continue;

                var paramResCap = memoryService.Read<nint>(tableBase + 0x70);
                if (paramResCap == 0) continue;

                var strLen = memoryService.Read<int>(paramResCap + 0x20);
                var nameAddr = strLen < 8
                    ? paramResCap + 0x10
                    : memoryService.Read<nint>(paramResCap + 0x10);
                if (nameAddr == 0) continue;

                var name = memoryService.ReadString(nameAddr);

                Console.WriteLine($"[{tableIndex}] {name}");
            }
        }

        public void Write<T>(nint row, int offset, T value) where T : unmanaged 
            => memoryService.Write(row + offset, value);

        private (nint paramData, int rowCount, nint descriptorBase)? GetParamData(int tableIndex)
        {
            if (tableIndex < 0 || tableIndex >= 0x61) return null;

            var soloParamRepo = memoryService.Read<nint>(SoloParamRepo.Base);
            if (soloParamRepo == 0) return null;

            var tableBase = soloParamRepo + tableIndex * 0x48;

            var paramResCap = memoryService.Read<nint>(tableBase + 0x70);
            if (paramResCap == 0) return null;

            var ptr1 = memoryService.Read<nint>(paramResCap + 0x68);
            if (ptr1 == 0) return null;

            var paramData = memoryService.Read<nint>(ptr1 + 0x68);
            if (paramData == 0) return null;

            var rowCount = memoryService.Read<ushort>(paramData + 0x0A);
            var descriptorOffset = memoryService.Read<int>(paramData - 0x10);
            var descriptorBase = paramData + ((descriptorOffset + 0xF) & ~0xF);

            return (paramData, rowCount, descriptorBase);
        }
    }
}
// 

namespace SilkySouls3.Interfaces;

public interface IParamService
{
    nint GetParamRow(int tableIndex, uint rowId);
    void PrintAllParamTableNames();
    void Write<T>(nint row, int offset, T value) where T : unmanaged;
}
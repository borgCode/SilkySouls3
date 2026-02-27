// 

using System.Diagnostics;

namespace SilkySouls3.Interfaces;

public interface IMemoryService
{
    public bool IsAttached { get; }
    public Process? TargetProcess { get; }
    public nint BaseAddress { get; }
    public int ModuleMemorySize { get; }
    
    string ReadString(nint addr, int maxLength = 32);
    byte[] ReadBytes(nint addr, int size);
    public nint FollowPointers(nint address, int[] offsets, bool readFinalPtr);
    T[] ReadArray<T>(nint addr, int count) where T : unmanaged;
    T Read<T>(nint addr) where T : unmanaged;
    string HexDump(nint addr, int size);
    
    void Write<T>(nint addr, T value) where T : unmanaged;
    void Write(nint addr, bool value);
    void WriteWString(nint addr, string value, int maxChars = 32);
    void WriteBytes(nint addr, byte[] val);
    
    void SetBitValue(nint addr, int flagMask, bool setValue);
    bool IsBitSet(nint addr, int flagMask);

    void RunThread(nint address, uint timeout = uint.MaxValue);

    void AllocateAndExecute(byte[] shellcode);
    void AllocCodeCave();

    nint AllocateMem(uint size);
    void FreeMem(nint addr);
    
    void StartAutoAttach();
}
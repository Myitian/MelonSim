using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MelonSim.Modes;

[StructLayout(LayoutKind.Sequential)]
ref struct RefList4<T> where T : allows ref struct
{
    public T V0;
    public T V1;
    public T V2;
    public T V3;
    public int Count;
    public T this[int index] // unsafe
    {
        get
        {
            Debug.Assert(index is >= 0 and < 4);
            return Unsafe.Add(ref V0, index);
        }
        set
        {
            Debug.Assert(index is >= 0 and < 4);
            Unsafe.Add(ref V0, index) = value;
        }
    }
    public void Add(T value) // unsafe
    {
        Debug.Assert(Count is >= 0 and < 4);
        this[Count++] = value;
    }
}
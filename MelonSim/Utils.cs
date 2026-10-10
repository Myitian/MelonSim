using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MelonSim;

static class Utils
{
    public static bool IsRefBelongTo<T>(ref readonly T value, Span<T> span)
    {
        ref readonly T p = ref MemoryMarshal.GetReference(span);
        nint byteOffset = Unsafe.ByteOffset(in p, in value);
        return byteOffset >= 0 && byteOffset < (span.Length * (nint)Unsafe.SizeOf<T>());
    }
    public static void Print(ReadOnlySpan<FieldStatus> fields, int lineSize)
    {
        int zSize = fields.Length / lineSize;
        for (int zIndex = 0; zIndex < zSize; zIndex++)
        {
            for (int xIndex = 0; xIndex < lineSize; xIndex++)
            {
                FieldStatus field = fields[zIndex * lineSize + xIndex];
                if (field == FieldStatus.Empty)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('.');
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('@');
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
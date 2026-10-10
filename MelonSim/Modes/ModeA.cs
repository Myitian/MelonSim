using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MelonSim.Modes;

struct ModeA : IMode<ModeA>
{
    Xoshiro256StarStar _random;
    public FieldStatus[] Fields { get; }
    public int[] StemIndices { get; }
    public static ReadOnlySpan<int> SizesForSimulation => [
        1, 2, 4, 5, 8,
        10, 20, 40, 50, 80,
        100, 200, 400, 500, 800,
        1000, 2000, 4000, 5000, 8000,
        10000, 20000, 40000, 50000, 80000,
        100000, 200000, 400000, 500000, 800000,
        1000000, 2000000, 4000000,
        10000000, 100000000, 1000000000];
    public static int FinalSize => 2000000000;

    public ModeA(int count, ulong seed)
    {
        Fields = new FieldStatus[count + 1];
        StemIndices = new int[count];
        _random = new(seed);
        for (int i = 0; i < StemIndices.Length; i++)
            StemIndices[i] = i;
    }
    public void Simulate()
    {
        Span<FieldStatus> fields = Fields;
        Span<int> stemIndices = StemIndices;
        fields.Clear();
        _random.Shuffle(stemIndices);
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        for (int i = 0; i < stemIndices.Length; i++)
        {
            int index = stemIndices[i];
            ref FieldStatus a = ref Unsafe.Add(ref p, index);
            ref FieldStatus b = ref Unsafe.Add(ref p, index + 1);
            Debug.Assert(Utils.IsRefBelongTo(in a, Fields));
            Debug.Assert(Utils.IsRefBelongTo(in b, Fields));
            if (a == FieldStatus.Empty)
            {
                if (b == FieldStatus.Empty)
                    Unsafe.Add(ref p, index + ((int)_random.Next() & 1)) = FieldStatus.Block;
                else
                    a = FieldStatus.Block;
            }
            else if (b == FieldStatus.Empty)
                b = FieldStatus.Block;
        }
    }
    public readonly void Stat(out int emptyCount, out int melonCount)
    {
        emptyCount = 0;
        melonCount = 0;
        int len = Fields.Length - 1;
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        for (int i = 0; i < len; i++)
        {
            if (Unsafe.Add(ref p, i) == FieldStatus.Empty)
                emptyCount++;
            else
                melonCount++;
        }
    }

    public static ModeA Create(int size, ulong seed)
        => new(size, seed);
    public static int GetRepeatCount(int size)
        => Math.Max(100000000 / size, 100);
}
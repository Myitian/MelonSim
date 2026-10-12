using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MelonSim.Modes;

struct Flory : IMode<Flory>
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

    public Flory(int count, ulong seed)
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
        ref int s = ref MemoryMarshal.GetReference(stemIndices);
        ref FieldStatus p = ref MemoryMarshal.GetReference(fields);
        _random.Shuffle(ref s, stemIndices.Length);
        for (int i = 0; i < stemIndices.Length; i++)
        {
            int index = Unsafe.Add(ref s, i);
            ref FieldStatus a = ref Unsafe.Add(ref p, index);
            ref FieldStatus b = ref Unsafe.Add(ref p, index + 1);
            if (a == FieldStatus.Empty && b == FieldStatus.Empty)
            {
                a = FieldStatus.Block;
                b = FieldStatus.Block;
            }
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

    public static Flory Create(int size, ulong seed)
        => new(size, seed);
    public static int GetRepeatCount(int size)
        => Math.Max(100000000 / size, 100);
}
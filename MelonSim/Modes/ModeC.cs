using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MelonSim.Modes;

struct ModeC : IMode<ModeC>
{
    Xoshiro256StarStar _random;
    public FieldStatus[] Fields { get; }
    public int[] StemIndices { get; }
    public int LineSize { get; }
    public static ReadOnlySpan<int> SizesForSimulation => [
        1, 2, 4, 8,
        10, 20, 40, 80,
        100, 200, 400, 800,
        1000, 2000, 4000, 8000,
        10000];
    public static int FinalSize => 20000;

    public ModeC(int count, ulong seed)
        : this(count, count, seed)
    {
    }
    public ModeC(int x, int z, ulong seed)
    {
        int xSize = x * 2 + 2;
        int zSize = z + 1;
        LineSize = x * 2;
        Fields = new FieldStatus[xSize * zSize];
        StemIndices = new int[LineSize * z];
        _random = new(seed);
        for (int zIndex = 0, offset = 0, i = 0; zIndex < z; zIndex++, offset += xSize)
        {
            for (int xIndex = 1; xIndex <= LineSize; xIndex++, i++)
                StemIndices[i] = offset + xIndex;
        }
    }
    public void Simulate()
    {
        int w = LineSize + 2;
        List4<int> list = new();
        Span<FieldStatus> fields = Fields;
        Span<int> stemIndices = StemIndices;
        fields.Clear();
        ref int s = ref MemoryMarshal.GetReference(stemIndices);
        ref FieldStatus p = ref MemoryMarshal.GetReference(fields);
        _random.Shuffle(ref s, stemIndices.Length);
        for (int i = 0; i < stemIndices.Length; i++)
        {
            list.Clear();
            int index = Unsafe.Add(ref s, i);
            int a;
            int b;
            int c;
            int d;
            if (((index % w) & 1) == 0)
            {
                a = index;
                b = index + w - 1;
                c = index + w;
                d = index + w + 1;
            }
            else
            {
                a = index - 1;
                b = index;
                c = index + 1;
                d = index + w;
            }
            Debug.Assert(Utils.IsRefBelongTo(in Unsafe.Add(ref p, a), Fields));
            Debug.Assert(Utils.IsRefBelongTo(in Unsafe.Add(ref p, b), Fields));
            Debug.Assert(Utils.IsRefBelongTo(in Unsafe.Add(ref p, c), Fields));
            Debug.Assert(Utils.IsRefBelongTo(in Unsafe.Add(ref p, d), Fields));
            if (Unsafe.Add(ref p, a) == FieldStatus.Empty)
                list.Add(a);
            if (Unsafe.Add(ref p, b) == FieldStatus.Empty)
                list.Add(b);
            if (Unsafe.Add(ref p, c) == FieldStatus.Empty)
                list.Add(c);
            if (Unsafe.Add(ref p, d) == FieldStatus.Empty)
                list.Add(d);
            int offset = 0;
            switch (list.Count)
            {
                case 0:
                    break;
                case 1:
                    Unsafe.Add(ref p, list[offset]) = FieldStatus.Block;
                    break;
                case 2:
                    offset = (int)_random.Next() & 1;
                    goto case 1;
                case 3:
                    offset = (int)_random.Next(3);
                    goto case 1;
                default:
                    offset = (int)_random.Next() & 3;
                    goto case 1;
            }
        }
    }
    public readonly void Stat(out int emptyCount, out int melonCount)
    {
        emptyCount = 0;
        melonCount = 0;
        int w = LineSize + 2;
        int len = Fields.Length - w;
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        for (int offset = 0; offset <= len; offset += w)
        {
            for (int i = 1; i <= LineSize; i++)
            {
                if (offset == ((i & 1) == 0 ? len : 0))
                    continue;
                if (Unsafe.Add(ref p, offset + i) == FieldStatus.Empty)
                    emptyCount++;
                else
                    melonCount++;
            }
        }
    }

    public static ModeC Create(int size, ulong seed)
        => new(size, seed);
    public static int GetRepeatCount(int size)
        => Math.Max(10000000 / size, 50);
}
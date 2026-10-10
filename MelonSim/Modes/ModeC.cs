using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
    void SimulateCore(int index)
    {
        RefList4<Ref<FieldStatus>> list = new();
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        ref FieldStatus a = ref Unsafe.NullRef<FieldStatus>();
        ref FieldStatus b = ref Unsafe.NullRef<FieldStatus>();
        ref FieldStatus c = ref Unsafe.NullRef<FieldStatus>();
        ref FieldStatus d = ref Unsafe.NullRef<FieldStatus>();
        int w = LineSize + 2;
        if (((index % w) & 1) == 0)
        {
            a = ref Unsafe.Add(ref p, index);
            b = ref Unsafe.Add(ref p, index + w - 1);
            c = ref Unsafe.Add(ref p, index + w);
            d = ref Unsafe.Add(ref p, index + w + 1);
        }
        else
        {
            a = ref Unsafe.Add(ref p, index - 1);
            b = ref Unsafe.Add(ref p, index);
            c = ref Unsafe.Add(ref p, index + 1);
            d = ref Unsafe.Add(ref p, index + w);
        }
        Debug.Assert(Utils.IsRefBelongTo(in a, Fields));
        Debug.Assert(Utils.IsRefBelongTo(in b, Fields));
        Debug.Assert(Utils.IsRefBelongTo(in c, Fields));
        Debug.Assert(Utils.IsRefBelongTo(in d, Fields));
        if (a == FieldStatus.Empty)
            list.Add(new(ref a));
        if (b == FieldStatus.Empty)
            list.Add(new(ref b));
        if (c == FieldStatus.Empty)
            list.Add(new(ref c));
        if (d == FieldStatus.Empty)
            list.Add(new(ref d));
        int offset = 0;
        switch (list.Count)
        {
            case 0:
                break;
            case 1:
                list[offset].Value = FieldStatus.Block;
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
    public void Simulate()
    {
        Span<FieldStatus> fields = Fields;
        Span<int> stemIndices = StemIndices;
        fields.Clear();
        _random.Shuffle(stemIndices);
        for (int i = 0; i < stemIndices.Length; i++)
            SimulateCore(stemIndices[i]);
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
        => Math.Max(100000000 / size, 50);
}
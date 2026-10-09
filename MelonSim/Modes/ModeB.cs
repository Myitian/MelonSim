using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MelonSim.Modes;

sealed class ModeB
{
    private Xoshiro256StarStar _random;
    public FieldStatus[] Fields { get; }
    public int[] StemIndices { get; }
    public int LineSize { get; }


    public ModeB(int count, ulong seed)
        : this(count, count, seed)
    {
    }
    public ModeB(int x, int z, ulong seed)
    {
        int xSize = x * 2 + 1;
        int zSize = z + 1;
        LineSize = x;
        Fields = new FieldStatus[xSize * zSize];
        StemIndices = new int[x * z];
        _random = new(seed);
        for (int zIndex = 0, offset = 2, i = 0; zIndex < z; zIndex++, offset++)
        {
            for (int xIndex = 0; xIndex < x; xIndex++, offset += 2, i++)
                StemIndices[i] = offset;
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
    void SimulateCore(int index)
    {
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        RefList4<Ref<FieldStatus>> list = new();
        ref FieldStatus a = ref Unsafe.Add(ref p, index - 2);
        ref FieldStatus b = ref Unsafe.Add(ref p, index - 1);
        ref FieldStatus c = ref Unsafe.Add(ref p, index);
        ref FieldStatus d = ref Unsafe.Add(ref p, LineSize * 2 + index);
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
    public static void Simulate(string resultFilePath)
    {
        File.WriteAllText(resultFilePath, "Size,Factor,Empty,Block,Ratio\n", Encoding.UTF8);
        Console.Out.Write("Size,Factor,Empty,Block,Ratio\n");
        ReadOnlySpan<int> sizes = [
            1, 2, 4, 8,
            10, 20, 40, 80,
            100, 200, 400, 800,
            1000, 2000, 4000, 8000,
            10000, 20000];
        foreach (int size in sizes)
            SimulateCore(resultFilePath, size, Math.Max(100000000 / (size * size), 50));
        SimulateCore(resultFilePath, 40000);
    }
    public static void SimulateCore(string resultFilePath, int size, int repeat = int.MaxValue, ulong seed = 114514)
    {
        Stopwatch sw = new();
        Console.Error.WriteLine("Initializing...");
        sw.Restart();
        ModeB b = new(size, seed);
        sw.Stop();
        Console.Error.WriteLine($"[INIT {size}] done in {sw.ElapsedMilliseconds}ms!");
        long totalEmptyCount = 0;
        long totalMelonCount = 0;
        size = b.StemIndices.Length;
        bool mergeStat = size < 10000000;
        if (mergeStat)
        {
            int factor = repeat;
            Console.Error.WriteLine("Processing...");
            sw.Restart();
            while (repeat-- > 0)
                SimulateWithMerge(b, ref totalEmptyCount, ref totalMelonCount);
            sw.Stop();
            Console.Error.WriteLine($"Done in {sw.ElapsedMilliseconds}ms!");
            double ratio = (double)totalEmptyCount / ((long)size * factor);
            Console.Error.WriteLine($"Size: {size}, Empty: {totalEmptyCount}, Block: {totalMelonCount}, Ratio: {ratio}");
            string log = $"{size},{factor},{totalEmptyCount},{totalMelonCount},{ratio}\n";
            File.AppendAllText(resultFilePath, log, Encoding.UTF8);
            Console.Out.Write(log);
        }
        else
        {
            while (repeat-- > 0)
                SimulateNoMerge(sw, b, resultFilePath);
        }

        static void SimulateWithMerge(ModeB b, ref long totalEmptyCount, ref long totalMelonCount)
        {
            b.Simulate();
            Stat(b, out int emptyCount, out int melonCount);
            totalEmptyCount += emptyCount;
            totalMelonCount += melonCount;
        }
        static void SimulateNoMerge(Stopwatch sw, ModeB b, string resultFilePath)
        {
            Console.Error.WriteLine("Processing...");
            sw.Restart();
            b.Simulate();
            sw.Stop();
            Console.Error.WriteLine($"Done in {sw.ElapsedMilliseconds}ms!");
            Stat(b, out int emptyCount, out int melonCount);
            int size = b.StemIndices.Length;
            double ratio = (double)emptyCount / size;
            Console.Error.WriteLine($"Size: {size}, Empty: {emptyCount}, Block: {melonCount}, Ratio: {ratio}");
            string log = $"{size},1,{emptyCount},{melonCount},{ratio}\n";
            File.AppendAllText(resultFilePath, log, Encoding.UTF8);
            Console.Out.Write(log);
        }
        static void Stat(ModeB b, out int emptyCount, out int melonCount)
        {
            emptyCount = 0;
            melonCount = 0;
            int w = b.LineSize * 2 + 1;
            int len = b.Fields.Length - w;
            ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(b.Fields);
            for (int offset = 0; offset < len; offset += w)
            {
                for (int i = 1; i < w; i++)
                {
                    if (Unsafe.Add(ref p, offset + i) == FieldStatus.Empty)
                        emptyCount++;
                    else
                        melonCount++;
                }
            }
        }
    }
    ref struct Ref<T>(ref T value)
    {
        public ref T Value = ref value;
    }
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
}
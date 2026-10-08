using System.Diagnostics;
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
        for (int i = 0; i < StemIndices.Length; i++)
            StemIndices[i] = i;
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
        Span<FieldStatus> fields = Fields;
        RefList4<FieldStatus> list = new();
        index *= 2;
        ref FieldStatus a = ref fields[index];
        ref FieldStatus b = ref fields[index + 1];
        ref FieldStatus c = ref fields[index + 2];
        ref FieldStatus d = ref fields[LineSize * 2 + index + 1];
        if (a == FieldStatus.Empty)
            list.Add(new(ref a));
        if (b == FieldStatus.Empty)
            list.Add(new(ref b));
        if (c == FieldStatus.Empty)
            list.Add(new(ref c));
        if (d == FieldStatus.Empty)
            list.Add(new(ref d));
        switch (list.Count)
        {
            case 0:
                break;
            case 1:
                list.V0.Value = FieldStatus.Block;
                break;
            case 2:
                (((int)_random.Next() & 1) == 0 ? list.V0 : list.V1).Value = FieldStatus.Block;
                break;
            case 3:
                (_random.Next(3) switch
                {
                    0 => list.V0,
                    1 => list.V1,
                    _ => list.V2,
                }).Value = FieldStatus.Block;
                break;
            default:
                (((int)_random.Next() & 3) switch
                {
                    0 => list.V0,
                    1 => list.V1,
                    2 => list.V2,
                    _ => list.V3,
                }).Value = FieldStatus.Block;
                break;
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
            SimulateCore(resultFilePath, size, Math.Max(10000000 / (size * size), 50));
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
            int emptyCount = 0;
            int melonCount = 0;
            for (int i = 0; i < b.Fields.Length - 1; i++)
            {
                switch (b.Fields[i])
                {
                    case FieldStatus.Empty:
                        emptyCount++;
                        break;
                    case FieldStatus.Block:
                        melonCount++;
                        break;
                }
            }
            emptyCount -= b.LineSize;
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
            int emptyCount = 0;
            int melonCount = 0;
            for (int i = 0; i < b.Fields.Length - 1; i++)
            {
                switch (b.Fields[i])
                {
                    case FieldStatus.Empty:
                        emptyCount++;
                        break;
                    case FieldStatus.Block:
                        melonCount++;
                        break;
                }
            }
            int size = b.StemIndices.Length;
            emptyCount -= b.LineSize;
            double ratio = (double)emptyCount / size;
            Console.Error.WriteLine($"Size: {size}, Empty: {emptyCount}, Block: {melonCount}, Ratio: {ratio}");
            string log = $"{size},1,{emptyCount},{melonCount},{ratio}\n";
            File.AppendAllText(resultFilePath, log, Encoding.UTF8);
            Console.Out.Write(log);
        }
    }
    ref struct Ref<T>(ref T value)
    {
        public ref T Value = ref value;
    }
    ref struct RefList4<T>
    {
        public int Count;
        public Ref<T> V0;
        public Ref<T> V1;
        public Ref<T> V2;
        public Ref<T> V3;
        public void Add(Ref<T> value)
        {
            int c = Count;
            switch (c)
            {
                case 0:
                    V0 = value;
                    break;
                case 1:
                    V1 = value;
                    break;
                case 2:
                    V2 = value;
                    break;
                case 3:
                    V3 = value;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            Count = c + 1;
        }
    }
}
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MelonSim.Modes;

sealed class ModeB_dbg
{
    private Xoshiro256StarStar _random;
    public FieldStatus[] Fields { get; }
    public int[] StemIndices { get; }
    public int LineSize { get; }


    public ModeB_dbg(int count, ulong seed)
        : this(count, count, seed)
    {
    }
    public ModeB_dbg(int x, int z, ulong seed)
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
        //_random.Shuffle(stemIndices);
        for (int i = 0; i < stemIndices.Length; i++)
            SimulateCore(stemIndices[i]);
    }
    void SimulateCore(int index)
    {
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        RefList4<FieldStatus> list = new();
        ref FieldStatus a = ref Unsafe.Add(ref p, index - 2);
        ref FieldStatus b = ref Unsafe.Add(ref p, index - 1);
        ref FieldStatus c = ref Unsafe.Add(ref p, index);
        ref FieldStatus d = ref Unsafe.Add(ref p, LineSize * 2 + index);
        if (a == FieldStatus.Empty)
        {
            list.Add(new(ref a));
            a = FieldStatus.Stem;
        }
        if (b == FieldStatus.Empty)
        {
            list.Add(new(ref b));
            b = FieldStatus.Stem;
        }
        if (c == FieldStatus.Empty)
        {
            list.Add(new(ref c));
            c = FieldStatus.Stem;
        }
        if (d == FieldStatus.Empty)
        {
            list.Add(new(ref d));
            d = FieldStatus.Stem;
        }
        Print();
        for (int i = 0; i < list.Count; i++)
            list[i].Value = FieldStatus.Empty;
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
        Print();
    }
    void Print()
    {
        ref FieldStatus p = ref MemoryMarshal.GetArrayDataReference(Fields);
        int w = LineSize * 2 + 1;
        int len = Fields.Length - w;
        for (int o = 0; o <= len; o += w)
        {
            for (int i = 0; i < w; i++)
            {
                switch (Unsafe.Add(ref p, o + i))
                {
                    case FieldStatus.Empty:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write('-');
                        break;
                    case FieldStatus.Block:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write('O');
                        break;
                    case FieldStatus.Stem:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write('*');
                        break;
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    public static void Simulate(string resultFilePath)
    {
        File.WriteAllText(resultFilePath, "Size,Factor,Empty,Block,Ratio\n", Encoding.UTF8);
        Console.Out.Write("Size,Factor,Empty,Block,Ratio\n");
        SimulateCore(resultFilePath, 5, 1);
    }
    public static void SimulateCore(string resultFilePath, int size, int repeat = int.MaxValue, ulong seed = 114514)
    {
        Stopwatch sw = new();
        Console.Error.WriteLine("Initializing...");
        sw.Restart();
        ModeB_dbg b = new(size, seed);
        sw.Stop();
        Console.Error.WriteLine($"[INIT {size}] done in {sw.ElapsedMilliseconds}ms!");
        size = b.StemIndices.Length;
        while (repeat-- > 0)
        {
            Console.Error.WriteLine("Processing...");
            sw.Restart();
            b.Simulate();
            sw.Stop();
            Console.Error.WriteLine($"Done in {sw.ElapsedMilliseconds}ms!");
            int emptyCount = 0;
            int melonCount = 0;
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
        public Ref<T> V0;
        public Ref<T> V1;
        public Ref<T> V2;
        public Ref<T> V3;
        public int Count;
        public Ref<T> this[int index]
        {
            get => Unsafe.Add(ref V0, index);
            set => Unsafe.Add(ref V0, index) = value;
        }
        public void Add(Ref<T> value)
        {
            int c = Count;
            this[c] = value;
            Count = c + 1;
        }
    }
}
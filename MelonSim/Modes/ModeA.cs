using System.Diagnostics;
using System.Text;

namespace MelonSim.Modes;

sealed class ModeA
{
    private Xoshiro256StarStar _random;
    public FieldStatus[] Fields { get; }
    public int[] StemIndices { get; }

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
        for (int i = 0; i < stemIndices.Length; i++)
            SimulateCore(stemIndices[i]);
    }
    void SimulateCore(int index)
    {
        Span<FieldStatus> fields = Fields;
        FieldStatus a = fields[index];
        FieldStatus b = fields[index + 1];
        if (a == FieldStatus.Empty)
        {
            if (b == FieldStatus.Empty)
                fields[index + ((int)_random.Next() & 1)] = FieldStatus.Block;
            else
                fields[index] = FieldStatus.Block;
        }
        else if (b == FieldStatus.Empty)
            fields[index + 1] = FieldStatus.Block;
    }
    public static void Simulate(string resultFilePath)
    {
        File.WriteAllText(resultFilePath, "Size,Factor,Empty,Block,Ratio\n", Encoding.UTF8);
        Console.Out.Write("Size,Factor,Empty,Block,Ratio\n");
        ReadOnlySpan<int> sizes = [
            1, 2, 4, 5, 8,
            10, 20, 40, 50, 80,
            100, 200, 400, 500, 800,
            1000, 2000, 4000, 5000, 8000,
            10000, 20000, 40000, 50000, 80000,
            100000, 200000, 400000, 500000, 800000,
            1000000, 2000000, 4000000,
            10000000, 100000000, 1000000000];
        foreach (int size in sizes)
            SimulateCore(resultFilePath, size, Math.Max(100000000 / size, 100));
        SimulateCore(resultFilePath, 2000000000);
    }
    public static void SimulateCore(string resultFilePath, int size, int repeat = int.MaxValue, ulong seed = 114514)
    {
        Stopwatch sw = new();
        Console.Error.WriteLine("Initializing...");
        sw.Restart();
        ModeA a = new(size, seed);
        sw.Stop();
        Console.Error.WriteLine($"[INIT] done in {sw.ElapsedMilliseconds}ms!");
        long totalEmptyCount = 0;
        long totalMelonCount = 0;
        bool mergeStat = size < 1000000;
        if (mergeStat)
        {
            int factor = repeat;
            while (repeat-- > 0)
                SimulateWithMerge(sw, a, ref totalEmptyCount, ref totalMelonCount);
            Console.Error.WriteLine($"[STAT] done in {sw.ElapsedMilliseconds}ms!");
            double ratio = (double)totalEmptyCount / (totalEmptyCount + totalMelonCount);
            Console.Error.WriteLine($"Size: {size}, Empty: {totalEmptyCount}, Block: {totalMelonCount}, Ratio: {ratio}");
            string log = $"{size},{factor},{totalEmptyCount},{totalMelonCount},{ratio}\n";
            File.AppendAllText(resultFilePath, log, Encoding.UTF8);
            Console.Out.Write(log);
        }
        else
        {
            while (repeat-- > 0)
                SimulateNoMerge(sw, a, resultFilePath, size);
        }

        static void SimulateWithMerge(Stopwatch sw, ModeA a, ref long totalEmptyCount, ref long totalMelonCount)
        {
            a.Simulate();
            int emptyCount = 0;
            int melonCount = 0;
            for (int i = 0; i < a.Fields.Length - 1; i++)
            {
                switch (a.Fields[i])
                {
                    case FieldStatus.Empty:
                        emptyCount++;
                        break;
                    case FieldStatus.Block:
                        melonCount++;
                        break;
                }
            }
            totalEmptyCount += emptyCount;
            totalMelonCount += melonCount;
        }
        static void SimulateNoMerge(Stopwatch sw, ModeA a, string resultFilePath, int size)
        {
            Console.Error.WriteLine("Processing...");
            sw.Restart();
            a.Simulate();
            sw.Stop();
            Console.Error.WriteLine($"Done in {sw.ElapsedMilliseconds}ms!");
            int emptyCount = 0;
            int melonCount = 0;
            for (int i = 0; i < a.Fields.Length - 1; i++)
            {
                switch (a.Fields[i])
                {
                    case FieldStatus.Empty:
                        emptyCount++;
                        break;
                    case FieldStatus.Block:
                        melonCount++;
                        break;
                }
            }
            double ratio = (double)emptyCount / (emptyCount + melonCount);
            Console.Error.WriteLine($"Size: {size}, Empty: {emptyCount}, Block: {melonCount}, Ratio: {ratio}");
            string log = $"{size},1,{emptyCount},{melonCount},{ratio}\n";
            File.AppendAllText(resultFilePath, log, Encoding.UTF8);
            Console.Out.Write(log);
        }
    }
}
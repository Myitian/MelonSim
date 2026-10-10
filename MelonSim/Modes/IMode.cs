using System.Diagnostics;
using System.Text;

namespace MelonSim.Modes;

interface IMode<T> where T : IMode<T>
{
    FieldStatus[] Fields { get; }
    int[] StemIndices { get; }

    void Simulate();
    void Stat(out int emptyCount, out int melonCount);
    static abstract T Create(int size, ulong seed);

    public static void Simulate(string resultFilePath, int size, int repeat = int.MaxValue, ulong seed = 114514)
    {
        Stopwatch sw = new();
        Console.Error.WriteLine("Initializing...");
        sw.Restart();
        T mode = T.Create(size, seed);
        sw.Stop();
        Console.Error.WriteLine($"[INIT {size}] done in {sw.ElapsedMilliseconds}ms!");
        long totalEmptyCount = 0;
        long totalMelonCount = 0;
        size = mode.StemIndices.Length;
        bool mergeStat = size < 10000000;
        if (mergeStat)
        {
            int factor = repeat;
            Console.Error.WriteLine("Processing...");
            sw.Restart();
            while (repeat-- > 0)
            {
                mode.Simulate();
                mode.Stat(out int emptyCount, out int melonCount);
                totalEmptyCount += emptyCount;
                totalMelonCount += melonCount;
            }
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
            {
                Console.Error.WriteLine("Processing...");
                sw.Restart();
                mode.Simulate();
                sw.Stop();
                Console.Error.WriteLine($"Done in {sw.ElapsedMilliseconds}ms!");
                mode.Stat(out int emptyCount, out int melonCount);
                double ratio = (double)emptyCount / size;
                Console.Error.WriteLine($"Size: {size}, Empty: {emptyCount}, Block: {melonCount}, Ratio: {ratio}");
                string log = $"{size},1,{emptyCount},{melonCount},{ratio}\n";
                File.AppendAllText(resultFilePath, log, Encoding.UTF8);
                Console.Out.Write(log);
            }
        }
    }
}
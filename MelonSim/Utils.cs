using MelonSim.Modes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MelonSim;

static class Utils
{
    public static bool IsRefBelongTo<T>(scoped ref readonly T value, scoped Span<T> span)
    {
        ref readonly T p = ref MemoryMarshal.GetReference(span);
        nint byteOffset = Unsafe.ByteOffset(in p, in value);
        return byteOffset >= 0 && byteOffset < (span.Length * (nint)Unsafe.SizeOf<T>());
    }
    public static void Print(scoped ReadOnlySpan<FieldStatus> fields, int lineSize)
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
    public static void Simulate<T>(string resultFilePath)
        where T : IMode<T>
    {
        File.WriteAllText(resultFilePath, "Size,Factor,Empty,Block,Ratio\n", Encoding.UTF8);
        Console.Out.Write("Size,Factor,Empty,Block,Ratio\n");
        foreach (int size in T.SizesForSimulation)
            SimulateOne<T>(resultFilePath, size, T.GetRepeatCount(size));
        SimulateOne<T>(resultFilePath, T.FinalSize);
    }
    public static void SimulateOne<T>(string resultFilePath, int size, int repeat = int.MaxValue, ulong seed = 114514)
        where T : IMode<T>
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
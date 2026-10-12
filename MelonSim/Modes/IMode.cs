namespace MelonSim.Modes;

interface IMode<T> where T : IMode<T>
{
    FieldStatus[] Fields { get; }
    int[] StemIndices { get; }
    static abstract ReadOnlySpan<int> SizesForSimulation { get; }
    static abstract int FinalSize { get; }

    void Simulate();
    void Stat(out int emptyCount, out int melonCount);
    static abstract T Create(int size, ulong seed);
    static abstract int GetRepeatCount(int size);
}
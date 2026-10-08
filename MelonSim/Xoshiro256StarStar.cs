using System.Numerics;

namespace MelonSim;

struct Xoshiro256StarStar(ulong seed)
{
    public ulong S0 = SplitMix64(ref seed);
    public ulong S1 = SplitMix64(ref seed);
    public ulong S2 = SplitMix64(ref seed);
    public ulong S3 = SplitMix64(ref seed);
    public ulong Next()
    {
        ulong s0 = S0, s1 = S1, s2 = S2, s3 = S3;

        ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;
        ulong t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;
        s3 = BitOperations.RotateLeft(s3, 45);

        S0 = s0;
        S1 = s1;
        S2 = s2;
        S3 = s3;

        return result;
    }
    public ulong Next(ulong upperBound)
    {
        ulong x = Next();
        UInt128 m = ulong.BigMul(x, upperBound);
        ulong l = (ulong)m;
        if (l < upperBound)
        {
            ulong t = (0 - upperBound) % upperBound;
            while (l < t)
            {
                x = Next();
                m = ulong.BigMul(x, upperBound);
                l = (ulong)m;
            }
        }
        return (ulong)(m >> 64);
    }
    public void Shuffle<T>(Span<T> values)
    {
        for (int i = 0; i < values.Length - 1; i++)
        {
            int j = i + (int)Next((ulong)(values.Length - i));
            T temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }
    public static ulong SplitMix64(ref ulong x)
    {
        ulong z = x += 0x9E3779B97F4A7C15;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EB;
        return z ^ (z >> 31);
    }
}
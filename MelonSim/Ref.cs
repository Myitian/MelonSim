namespace MelonSim;

ref struct Ref<T>(ref T value)
{
    public ref T Value = ref value;
}
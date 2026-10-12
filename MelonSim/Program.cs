using MelonSim;
using MelonSim.Modes;

string resultFilePath;
string mode;
if (args.Length > 0)
    mode = args[0].Trim().ToLowerInvariant();
else
    mode = "c";
if (args.Length > 1)
    resultFilePath = args[1];
else
    resultFilePath = $"{mode}-stat.csv";
switch (mode)
{
    case "a":
        Utils.Simulate<ModeA>(resultFilePath);
        break;
    case "b":
        Utils.Simulate<ModeB>(resultFilePath);
        break;
    case "c":
        Utils.Simulate<ModeC>(resultFilePath);
        break;
    case "f":
        Utils.Simulate<Flory>(resultFilePath);
        break;
    default:
        Console.Error.WriteLine("Unknown mode: {0}", mode);
        return;
}
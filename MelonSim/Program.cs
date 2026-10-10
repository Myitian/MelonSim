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
        ModeA.Simulate(resultFilePath);
        break;
    case "b":
        ModeB.Simulate(resultFilePath);
        break;
    case "c":
        ModeC.Simulate(resultFilePath);
        break;
    case "f":
        Flory.Simulate(resultFilePath);
        break;
    default:
        Console.Error.WriteLine("Unknown mode: {0}", mode);
        return;
}
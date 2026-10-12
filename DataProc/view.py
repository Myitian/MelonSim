import matplotlib.pyplot as plt
import pandas as pd

for mode in ("a", "b", "c"):
    pd.set_option("display.precision", 10)
    stat = pd.read_csv(f"DataProc/{mode}-stat.csv")
    p1 = stat.groupby("Size")[["Factor", "Block"]].sum()
    p1["Ratio"] = p1["Block"] / (p1.index * p1["Factor"])
    print(p1)
    plt.figure(figsize=(8, 5), dpi=100)
    plt.plot(p1.index, p1["Ratio"])
    plt.xscale("log")
    plt.xlabel("Stem")
    plt.ylabel("Block/Stem Ratio")
    plt.savefig(f"DataProc/{mode}-ratio.png", dpi=300)
    plt.show()

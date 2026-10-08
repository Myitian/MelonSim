import matplotlib.pyplot as plt
import pandas as pd

stat = pd.read_csv("DataProc/a-stat.csv")
p1 = stat.groupby("Size")[["Empty", "Block"]].sum()
p1["Ratio"] = p1["Block"] / (p1["Empty"] + p1["Block"])
print(p1)
plt.plot(p1.index, p1["Ratio"])
plt.xscale("log")
plt.xlabel("Stem")
plt.ylabel("Block/Stem Ratio")
plt.savefig("DataProc/a-ratio.png", dpi=300)
plt.show()

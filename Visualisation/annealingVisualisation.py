import glob
import json
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt

def get_hours(time_str : str) -> float:
    """Get hours from time."""
    h, m, s = time_str.split(':')
    s = s.split('.')[0]
    return int(h)  + int(m) / 60 + int(s) / 3600

mask = 'C:/Users/Mustrum/Dropbox/MDP/annealing_optimization/**/*Stats.json'
files = [f for f in glob.glob(mask, recursive=True)]

df = pd.DataFrame(index=np.arange(0, len(files)), columns=('alpha', 'beta', 'value', 'time') )

for i, f in enumerate(files):
    with open(f) as json_file:
        data = json.load(json_file)
        alpha = data['Solver']['TemperatureMultiplier']
        beta = data['Solver']['TemperatureAddition']
        loss = data['TasksStats']['WeightedLoss']
        time = get_hours(data['TotalTime'])
        df.loc[i] = [alpha, beta, loss, time]

df.sort_index(inplace=True)
df = df.reindex(sorted(df.columns), axis=1)
fig, axes = plt.subplots(ncols=2)
fig.set_size_inches(10, 5)

def preparePivotTable(df : pd.DataFrame) -> pd.DataFrame:
    df.sort_index(inplace=True)
    df = df.reindex(sorted(df.columns), axis=1)
    df.columns = [f"1 - {1 - val:.2E}" for val in df.columns]
    df.index = df.index.map(lambda val : f"{val:.2E}")
    return df


lossPivot = df.groupby(['beta', 'alpha'])['value'].sum().unstack('alpha')
lossPivot = preparePivotTable(lossPivot)
sns.heatmap(lossPivot,
    cbar_kws={'label': 'weighted loss'}, 
    cmap = sns.cm.rocket_r, ax=axes[0])
timePivot = df.groupby(['beta', 'alpha'])['time'].sum().unstack('alpha')
timePivot = preparePivotTable(timePivot)
sns.heatmap(timePivot, 
    cbar_kws={'label': 'time [h]'}, 
    cmap = "YlGnBu", ax=axes[1])

# for ax in axes:
#     ax.set_xlabel("alpha")
#     ax.set_ylabel("beta")
plt.tight_layout()
plt.show()
#print(df)
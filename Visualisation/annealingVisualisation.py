import glob
import json
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle, Patch
import re

def get_minutes(time_str : str) -> float:
    """Get hours from time."""
    h, m, s = time_str.split(':')
    s = s.split('.')[0]
    return int(h) * 60  + int(m) + int(s) / 60

def get_hours(time_str : str) -> float:
    """Get hours from time."""
    h, m, s = time_str.split(':')
    s = s.split('.')[0]
    return int(h)  + int(m) / 60 + int(s) / 3600

directory = 'C:/Users/Mustrum/Dropbox/MDP/annealing_optimization/'
fileMask = '**/*Stats.json'
files = [f for f in glob.glob(directory + fileMask, recursive=True)]

df = pd.DataFrame(index=np.arange(0, len(files)), columns=('alpha', 'beta', 'loss', 'time[min]') )

for i, f in enumerate(files):
    with open(f) as json_file:
        data = json.load(json_file)
        alpha = data['Solver']['TemperatureMultiplier']
        beta = data['Solver']['TemperatureAddition']
        loss = data['TasksStats']['WeightedLoss'] / 30
        time = get_minutes(data['TotalTime']) / 30
        df.loc[i] = [alpha, beta, loss, time]

df.sort_index(inplace=True)
df = df.reindex(sorted(df.columns), axis=1)
fig, axes = plt.subplots(ncols=2)
fig.set_size_inches(10, 5)

def preparePivotTable(df : pd.DataFrame) -> pd.DataFrame:
    df.sort_index(inplace=True)
    df = df.reindex(sorted(df.columns), axis=1)
    df.columns = [f"1 - {1 - val:.2E}".replace("E-0", "E-") for val in df.columns]
    df.index = df.index.map(lambda val : f"{val:.2E}".replace("E-0", "E-"))
    return df

df.sort_values('loss', inplace=True)
df.to_csv (directory + 'lossSorted.csv', index = None, header=True)
df.head(5).to_csv (directory + 'bestLoss.csv', index = None, header=True)
df.tail(5).to_csv (directory + 'worstLoss.csv', index = None, header=True)

df.sort_values('time[min]', inplace=True)
df.to_csv (directory + 'timeSorted.csv', index = None, header=True)
df.head(5).to_csv (directory + 'bestTime.csv', index = None, header=True)
df.tail(5).to_csv (directory + 'worstTime.csv', index = None, header=True)

lossPivot = df.groupby(['beta', 'alpha'])['loss'].sum().unstack('alpha')
lossPivot = preparePivotTable(lossPivot)
sns.heatmap(lossPivot,
    cbar_kws={'label': 'loss average'}, 
    cmap = sns.cm.rocket_r, ax=axes[0])
timePivot = df.groupby(['beta', 'alpha'])['time[min]'].sum().unstack('alpha')
timePivot = preparePivotTable(timePivot)
sns.heatmap(timePivot, 
    cbar_kws={'label': 'time [min] average'}, 
    cmap = "YlGnBu", ax=axes[1])

axes[0].set_title("Average loss for tested parameters\n(brighter is better)")
axes[1].set_title("Average time spent to generate a solution\n(brighter is better)")


for ax in axes:
    if 'annealing_optimization/' in directory:
        ax.add_patch(Rectangle((3, -1), 4.5, 6, fill=False, edgecolor='green', lw=2))
        legend_elements = [Patch(edgecolor='green', facecolor='None', lw=2, label='follow-up experiment area')]
        ax.legend(handles=legend_elements, loc='lower right')
    if 'annealing_optimization2/' in directory:
        ax.add_patch(Rectangle((4, 3), 5, 4, fill=False, edgecolor='green', lw=2))
        legend_elements = [Patch(edgecolor='green', facecolor='None', lw=2, label='final experiment area')]
        ax.legend(handles=legend_elements, loc='lower right')
    ax.set_xlabel("alpha")
    ax.set_ylabel("beta")
plt.tight_layout()
plt.show()
#print(df)
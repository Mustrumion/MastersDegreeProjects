import glob
import json
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
import math
import os
from matplotlib import rcParams
# rcParams.update({'figure.autolayout': True})

def get_hours(time_str : str) -> float:
    """Get hours from time."""
    h, m, s = time_str.split(':')
    s = s.split('.')[0]
    return int(h)  + int(m) / 60 + int(s) / 3600

def get_minutes(time_str : str) -> float:
    """Get hours from time."""
    h, m, s = time_str.split(':')
    s = s.split('.')[0]
    return int(h) * 60  + int(m) + int(s) / 60


def get_difficulty_from_path(path: str) -> int:
    if "extreme" in path:
        return 6
    if "hard" in path:
        return 5
    if "medium" in path:
        return 4
    if "\\easy\\" in path:
        return 3
    if "very_easy\\" in path:
        return 2
    if "trivial"in path:
        return 1

def get_length_from_path(path: str) -> str:
    if "week" in path:
        return "week"
    if "month" in path:
        return "month"

def get_channels_from_path(path: str):
    segments = os.path.split(path)
    char = os.path.split(segments[0])[1][0]
    return int(char) 


directory = 'C:/Users/Mustrum/Dropbox/MDP/solutions/annealing_best_tests_all2/'
fileMask = '**/0*.json'
files = [f for f in glob.glob(directory + fileMask, recursive=True)]

columns=('difficulty', 'horizon', 'channel count', 'accept. function', 'accept. f. % improv.', 'acceptable %', 'loss [x1000]', 'loss % improvement', 'time [min]', 'iterations [x100 000]')
df = pd.DataFrame(index=np.arange(0, len(files)), columns = columns)

for i, f in enumerate(files):
    with open(f) as json_file:
        data = json.load(json_file)
        loss = data['WeightedLoss'] / 1000
        lossPercent = (data['WeightedLossBefore'] - data['WeightedLoss']) / data['WeightedLossBefore'] * 100
        accetability = data['IntegrityLossScore']
        if data['IntegrityLossScoreBefore'] == 0:
            accetabilityPercent = float('nan')
        else:
            accetabilityPercent = (data['IntegrityLossScoreBefore'] - data['IntegrityLossScore']) / data['IntegrityLossScoreBefore'] * 100
        accetabilePercent = (data['IntegrityLossScore'] == 0) * 100.0
        time = get_minutes(data['LastTimeElapsed'])
        iterations = data['NumberOfIterations'] / 100000

        difficulty = get_difficulty_from_path(f)
        horizon = get_length_from_path(f)
        channels = get_channels_from_path(f)

        df.loc[i] = [difficulty, horizon, channels,
                accetability, accetabilityPercent, accetabilePercent, loss, lossPercent, time, iterations]

# df = df.apply(pd.to_numeric)
df.sort_values(by=['horizon'], inplace=True, ascending=False)
df.to_csv (directory + 'gatheredSolutionsData.csv', index = None, header=True)
print(df)
g = sns.PairGrid(df, x_vars=columns[0:3], y_vars=columns[3:])
g = g.map(sns.barplot, ci = 75)
plt.subplots_adjust(top=0.95, bottom=0.05)
g.fig.suptitle("Simulated annealing results categorized by the instances")
# plt.tight_layout()
plt.show()

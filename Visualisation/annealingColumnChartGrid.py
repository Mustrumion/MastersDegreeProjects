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

columns=['name', 'difficulty', 'horizon', 'channel count', 'accept. function', 'accept. f. % improv.', 'acceptable %', 'loss [x1000]', 'loss % improvement', 'time [min]', 'iterations [x100 000]']
df = pd.DataFrame(columns = columns)
outliers = pd.DataFrame(columns = columns)

for i, f in enumerate(files):
    with open(f) as json_file:
        pathSegments = os.path.split(f)
        fileName = os.path.splitext(pathSegments[1])[0]
        parentDir = os.path.split(pathSegments[0])[1]
        name = parentDir + "/" + fileName

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

        if time > 59.99:
            print(f)
            outliers.loc[i] = [name, difficulty, horizon, channels,
                accetability, accetabilityPercent, accetabilePercent, loss, lossPercent, time, iterations]
        else:
            df.loc[i] = [name, difficulty, horizon, channels,
                accetability, accetabilityPercent, accetabilePercent, loss, lossPercent, time, iterations]

df[columns[1:4]] = df[columns[1:4]].astype(str) 
df[columns[4:]] = df[columns[4:]].apply(pd.to_numeric)
# df = df.apply(pd.to_numeric)
df.sort_values(by=['horizon', 'name'], inplace=True, ascending=[False, True])
outliers.sort_values(by=['horizon', 'name'], inplace=True, ascending=[False, True])
df.to_csv (directory + 'plottedSolutionsData.csv', index = None, header=True)
outliers[['loss [x1000]', 'loss % improvement', 'iterations [x100 000]']]= outliers[['loss [x1000]', 'loss % improvement', 'iterations [x100 000]']].round(3)
outliers.to_csv (directory + 'outliersSolutionsData.csv', index = None, header=True)
print(df)
g = sns.PairGrid(df, x_vars=columns[1:4], y_vars=columns[4:])
g = g.map(sns.barplot)
plt.subplots_adjust(top=0.95, bottom=0.05)
g.fig.suptitle("Simulated annealing results categorized by the instances")
# plt.tight_layout()
plt.show()

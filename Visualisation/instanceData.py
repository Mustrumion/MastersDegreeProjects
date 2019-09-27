import glob
import json
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
import math
import os

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


directory = 'C:/Users/Mustrum/Dropbox/MDP/instances_with_stats/extreme/'
fileMask = '**/*.json'
files = [f for f in glob.glob(directory + fileMask, recursive=True)]

columns=('name', 'horizon', 'channel count', 'break count', 'avg. break [min]', 'campaigns', 'total ads min (dif 6)')
df = pd.DataFrame(index=np.arange(0, len(files)), columns = columns)

for i, f in enumerate(files):
    with open(f) as json_file:
        pathSegments = os.path.split(f)
        fileName = os.path.splitext(pathSegments[1])[0]
        parentDir = os.path.split(pathSegments[0])[1]
        name = parentDir + "/" + fileName

        data = json.load(json_file)
        horizon = data['HorizonType']
        channels = data['NumberOfChannels']
        breaks = data['NumberOfBreaks']
        breakAvg = data['AverageBreakLengthMinutes']
        campaigns = data['NumberOfCampaigns']
        ads = data['NumberOfAdsToSchedule']

        df.loc[i] = [name, horizon, channels, breaks, breakAvg, campaigns, ads]

# df = df.apply(pd.to_numeric)
df['avg. break [min]'] = pd.to_numeric(df['avg. break [min]'])
df['avg. break [min]']= round(df['avg. break [min]'], 1)
df.sort_values(['horizon', 'channel count', 'break count'], ascending=[False, True, True], inplace=True)
df.to_csv (directory + 'gatheredInstancesData.csv', index = None, header=True)
print(df)

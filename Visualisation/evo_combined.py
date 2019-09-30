import pathlib
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib
import numpy as np
import os

seriesNum = 0
count = 0
cmap = matplotlib.cm.get_cmap('Accent')
mainDimensions = ['Time [s]','Average size adjusted loss']
adjustToSize = True
fieldNames = ['Moves performed', 'Time [s]', 'Integrity loss', 'Average size adjusted loss', 'Acceptable', 'Action']

seriesFolders = ['solutions/annealing_best', 'solutions/evolutionary_best_2']
seriesNames = ['simulated annealing', 'evolutionary algorithm']

# This assumes the file and solution naming convention doesn't change
def GetSolutionSizeFromPath(path: str):
    segments = os.path.split(path)
    filename = segments[1]
    size = 1.0
    if 'month' in filename:
        size *= 4
    char = os.path.split(segments[0])[1][0]
    return size * int(char) 

def GetFileData(csvFile: str) -> pd.DataFrame:
    df = pd.read_csv(csvFile, header=None, names=fieldNames)
    df = df[df['Action'] != 'Started Evolutionary']
    df = df[mainDimensions]
    maxTime = df['Time [s]'].max()
    df = df.groupby(pd.cut(df[mainDimensions[0]], np.arange(0, maxTime, 0.25))).agg({mainDimensions[1]: {'max'}})
    df.columns = ["_".join(x) for x in df.columns.ravel()]
    for i in range(1, len(df)):
        if np.isnan(df.loc[i, mainDimensions[1]+'_max']):
            df.at[i, mainDimensions[1]+'_max'] = df.loc[i-1, mainDimensions[1]+'_max']
    for i in range(len(df)-1, -1, -1):
        if np.isnan(df.loc[i, mainDimensions[1]+'_max']):
            df.at[i, mainDimensions[1]+'_max'] = df.loc[i+1, mainDimensions[1]+'_max']
    if adjustToSize:
        df[mainDimensions[1]+'_max'] = df[[mainDimensions[1]+'_max']] / GetSolutionSizeFromPath(csvFile)
    df['Count'] = 1.0
    return df


def LoadAllData(folder : str, mainDimensions : pd.DataFrame, color) -> pd.DataFrame:
    dataframes = []
    for csvFile in pathlib.Path(folder).glob('**/*.csv'):
        dataframes.append(GetFileData(csvFile))
    chosenOne = dataframes[0]
    for frame in dataframes[:-1]:
        if len(frame) > len(chosenOne):
            lastValue = chosenOne.tail(1)[mainDimensions[1]+'_max'].iloc[0]
        else:
            lastValue = frame.tail(1)[mainDimensions[1]+'_max'].iloc[0]
        chosenOne = chosenOne.add(frame, fill_value = lastValue)
    global count 
    count = len(dataframes)
    chosenOne[mainDimensions[1]] = chosenOne[[mainDimensions[1]+'_max']] / len(dataframes)#.div(chosenOne['Count'].values, axis=0)
    chosenOne = chosenOne.drop([mainDimensions[1]+'_max', 'Count'], axis=1)
    chosenOne.reset_index(level=0, inplace=True)
    chosenOne['Time [s]'] = chosenOne['Time [s]'].apply(lambda x: x.left)
    return chosenOne

    
def plotAsScatter(dataframe, color = 'blue', seriesName = None):
    array = dataframe.values
    plt.plot(array[:,0], array[:,1], marker='o', linestyle='-', markersize = 1, label = seriesName, color = color)
    plt.xlabel(dataframe.columns.values[0])
    plt.ylabel(dataframe.columns.values[1] + " average")


for i, folder in enumerate(seriesFolders):
    df = LoadAllData(folder, mainDimensions, cmap(seriesNum))
    nameAlone = os.path.splitext(folder)[1]
    plotAsScatter(df, cmap(seriesNum), seriesNames[i])
    seriesNum += 1

plt.title(f"{mainDimensions[1]} over time, average from {count} runs")
plt.legend()
plt.show()
import pathlib
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib
import numpy as np
import os

maxTime = 900
seriesNum = 0
count = 0
cmap = matplotlib.cm.get_cmap('Accent')
mainDimensions = ['Time [s]','Integrity loss']
fieldNames = ['Moves performed', 'Time [s]', 'Integrity loss', 'Weighted loss', 'Acceptable', 'Action']

seriesFolders = ['solutions/compound_no_incom', 'solutions/compound_rand']
seriesNames = ['compound after random', 'compound after no incompatibilities heuristic']

def GetFileData(csvFile: str) -> pd.DataFrame:
    df = pd.read_csv(csvFile, header=None, names=fieldNames)
    df = df[mainDimensions]
    maxTime = df['Time [s]'].max()
    df = df.groupby(pd.cut(df[mainDimensions[0]], np.arange(0, maxTime, 0.25))).agg({mainDimensions[1]: {'max'}})
    df.columns = ["_".join(x) for x in df.columns.ravel()]
    for i in range(1, len(df)):
        if np.isnan(df.loc[i, mainDimensions[1]+'_max']):
            df.at[i, mainDimensions[1]+'_max'] = df.loc[i-1, mainDimensions[1]+'_max']
    df['Count'] = 1.0
    return df


def LoadAllData(folder : str, mainDimensions : pd.DataFrame, color) -> pd.DataFrame:
    dataframes = []
    for csvFile in pathlib.Path(folder).glob('**/*.csv'):
        dataframes.append(GetFileData(csvFile))
    chosenOne = dataframes[0]
    for frame in dataframes[:-1]:
        chosenOne = chosenOne.add(frame, fill_value = 0)
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
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib
import glob, os

nth = 50

n = 0
cmap = matplotlib.cm.get_cmap('Paired')
curryMax = 0
currxMax = 0
mainDimensions = ['Time [s]','Integrity loss']

# Preview the first 5 lines of the loaded data 

def plotDataframe(dataframe, color = 'blue'):
    array = dataframe.values
    plt.scatter(array[::nth,0], array[::nth,1], color = color)
    plt.xlabel(dataframe.columns.values[0])
    plt.ylabel(dataframe.columns.values[1])
    global curryMax, currxMax
    curryMax = max([curryMax, max(array[::nth,1])])
    currxMax = max([currxMax, max(array[::nth,0])])

def plotAlgData(algorithmsData = [], color = 'blue'):
    for algorithm in algorithmsData.values:
        plt.axvline(algorithm[0], label=algorithm[1], color = color)
        plt.text(algorithm[0] - currxMax*0.02, curryMax, algorithm[1], rotation=90, color = color)

def plotResults(filename, mainDimensions, color1, color2):
    df = pd.read_csv(filename, header=None, names=['Moves performed', 'Time [s]', 'Integrity loss', 'Wrighted loss', 'Acceptable', 'Action']) 
    progress = df[~df['Action'].str.startswith('Started')]
    algorithms = df[df['Action'].str.startswith('Started')]

    mainData = progress[mainDimensions]

    algorithmsData = algorithms[[mainDimensions[0], 'Action']]

    plotDataframe(mainData, color = color1)
    plotAlgData(algorithmsData, color = color2)

    columns = mainData.columns.values


for file in glob.glob("*.csv"):
    plotResults(file, mainDimensions, cmap(n), cmap(n + 1))
    n += 2

plt.title("{} over {}, every {}th transformation".format(mainDimensions[1], mainDimensions[0], nth))
plt.legend()
plt.show()
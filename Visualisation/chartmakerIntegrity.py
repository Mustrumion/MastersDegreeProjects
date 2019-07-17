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

def plotAsScatter(dataframe, color = 'blue', seriesName = None):
    array = dataframe.values
    plt.scatter(array[::nth,0], array[::nth,1], label = seriesName, color = color)
    plt.xlabel(dataframe.columns.values[0])
    plt.ylabel(dataframe.columns.values[1])
    global curryMax, currxMax
    curryMax = max([curryMax, max(array[::nth,1])])
    currxMax = max([currxMax, max(array[::nth,0])])

def plotAsLines(algorithmsData = [], color = 'blue'):
    for algorithm in algorithmsData:
        plt.axvline(algorithm[0], color = color)
        plt.text(algorithm[0] - currxMax*0.02, curryMax, algorithm[1], rotation=90, color = color)

def plotResults(filename, mainDimensions, color1, color2):
    base=os.path.basename(filename)
    nameAlone = os.path.splitext(base)[0]
    df = pd.read_csv(filename, header=None, names=['Moves performed', 'Time [s]', 'Integrity loss', 'Weighted loss', 'Acceptable', 'Action']) 
    progress = df[~df['Action'].str.startswith('Started')]
    algorithmsStarts = df[df['Action'].str.startswith('Started')]
    algorithmsEnds = df[df['Action'].str.startswith('Ended')]
    acceptabilityReached = df[df['Acceptable']]

    print(acceptabilityReached.head())

    mainData = progress[mainDimensions]
    mainData2 = algorithmsEnds[mainDimensions]
    startsData = algorithmsStarts[[mainDimensions[0], 'Action']]

    plotAsScatter(mainData, color = color1, seriesName = nameAlone)
    
    if(algorithmsEnds.shape[0] > 0):
        plotAsScatter(mainData2, color = color1)
        
    plotAsLines(startsData.values, color = color2)

    if(acceptabilityReached.shape[0] > 0):
        plotAsLines([[acceptabilityReached.iloc[0][1], "Reached acceptable solution"]], color = color2)


for file in glob.glob("*.csv"):
    plotResults(file, mainDimensions, cmap(n), cmap(n + 1))
    n += 2

plt.ylim(0, curryMax)
plt.title("{} over {}, every {}th transformation".format(mainDimensions[1], mainDimensions[0], nth))
plt.legend()
plt.show()
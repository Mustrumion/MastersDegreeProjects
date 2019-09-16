import glob
import json
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt
import math

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

directory = 'C:/Users/Mustrum/Dropbox/MDP/evolutionary_optimization/'
fileMask = '**/*Stats.json'
files = [f for f in glob.glob(directory + fileMask, recursive=True)]

df = pd.DataFrame(index=np.arange(0, len(files)), columns=('loss', 'time[min]', 'population count', 'parent candidates', 'mutation rate', 'mutation probability', 'crossover rate', 'crossover probability') )

for i, f in enumerate(files):
    with open(f) as json_file:
        data = json.load(json_file)
        number = data['NumberOfExamples']
        population = data['Solver']['PopulationCount']
        parentCandidates = data['Solver']['CandidatesForParent']
        mutationRate = data['Solver']['MutationRate']
        mutationProbability = data['Solver']['NumberOfMutationsToBreakCount']
        crossoverRate = data['Solver']['CrossoverRate']
        crossoverProbability = data['Solver']['ProportionOfBreaksCrossed']
        loss = data['TasksStats']['WeightedLoss'] / number
        time = get_minutes(data['TotalTime']) / number
        df.loc[i] = [loss, time, population, parentCandidates, mutationRate, mutationProbability, crossoverRate, crossoverProbability]

df = df.apply(pd.to_numeric)

df.sort_values('loss', inplace=True)
df.to_csv (directory + 'lossSorted.csv', index = None, header=True)
df.sort_values('time[min]', inplace=True)
df.to_csv (directory + 'timeSorted.csv', index = None, header=True)
df = df.round({'loss': 1, 'time[min]': 2, 'population count': 0, 'parent candidates': 0, 'mutation rate': 3, 'mutation probability': 5, 'crossover rate': 3, 'crossover probability': 5})
df.head(5).to_csv (directory + 'bestTime.csv', index = None, header=True)
df.tail(5).to_csv (directory + 'worstTime.csv', index = None, header=True)
df.sort_values('loss', inplace=True)
df.head(5).to_csv (directory + 'bestLoss.csv', index = None, header=True)
df.tail(5).to_csv (directory + 'worstLoss.csv', index = None, header=True)

df['mutation probability [log10]'] = df['mutation probability'].apply(math.log10)
df['crossover probability [log10]'] = df['crossover probability'].apply(math.log10)

#fig, axes = plt.subplots(ncols=2)
#fig.set_size_inches(10, 5)
# sns.lmplot(x="parentCandidates", y="value", data=df, x_estimator=np.mean)
# sns.lmplot(x="parentCandidates", y="time[min]", data=df, x_estimator=np.mean)

g = sns.pairplot(df, kind="reg", x_vars=['population count', 'mutation rate', 'crossover rate'], y_vars=['loss', 'time[min]'], 
        plot_kws=dict(line_kws=dict(lw=1), scatter_kws=dict(s=2), order=2))
axes = g.axes
for row in axes:
    for ax in row:
        ax.set_ylim(0,)
g = sns.pairplot(df, kind="reg", x_vars=['parent candidates', 'mutation probability [log10]', 'crossover probability [log10]'], y_vars=['loss', 'time[min]'], 
        plot_kws=dict(line_kws=dict(lw=1), scatter_kws=dict(s=2), order=2))
axes = g.axes
for row in axes:
    for ax in row:
        ax.set_ylim(0,)
plt.show()

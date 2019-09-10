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

df = pd.DataFrame(index=np.arange(0, len(files)), columns=('loss', 'time[min]', 'populationCount', 'parentCandidates', 'mutationRate', 'mutationProbability [log10]', 'crossoverRate', 'crossoverProbability [log10]') )

for i, f in enumerate(files):
    with open(f) as json_file:
        data = json.load(json_file)
        number = data['NumberOfExamples']
        population = data['Solver']['PopulationCount']
        parentCandidates = data['Solver']['CandidatesForParent']
        mutationRate = data['Solver']['MutationRate']
        mutationProbability = math.log10(data['Solver']['NumberOfMutationsToBreakCount'])
        crossoverRate = data['Solver']['CrossoverRate']
        crossoverProbability = math.log10(data['Solver']['ProportionOfBreaksCrossed'])
        loss = data['TasksStats']['WeightedLoss'] / number
        time = get_minutes(data['TotalTime']) / number
        df.loc[i] = [loss, time, population, parentCandidates, mutationRate, mutationProbability, crossoverRate, crossoverProbability]

df.sort_values('loss', inplace=True)
df.to_csv (directory + 'lossSorted.csv', index = None, header=True)
df.sort_values('time[min]', inplace=True)
df.to_csv (directory + 'timeSorted.csv', index = None, header=True)

df = df.apply(pd.to_numeric)

#fig, axes = plt.subplots(ncols=2)
#fig.set_size_inches(10, 5)
# sns.lmplot(x="parentCandidates", y="value", data=df, x_estimator=np.mean)
# sns.lmplot(x="parentCandidates", y="time[min]", data=df, x_estimator=np.mean)

g = sns.pairplot(df, kind="reg", x_vars=['populationCount', 'mutationRate', 'crossoverRate'], y_vars=['loss', 'time[min]'], plot_kws=dict(scatter_kws=dict(s=4), order=2))
g = sns.pairplot(df, kind="reg", x_vars=['parentCandidates', 'mutationProbability [log10]', 'crossoverProbability [log10]'], y_vars=['loss', 'time[min]'], plot_kws=dict(scatter_kws=dict(s=4), order=2))
plt.show()

import numpy as np
import pandas as pd

data = pd.read_csv("coursework/docs/papers/iterations.csv")
values = data['Loss']

mean_value = np.mean(values)
std_deviation = np.std(values)

print(f"{mean_value:.5f} ± {std_deviation:.5f}")

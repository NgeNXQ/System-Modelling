import pandas as pd

df: pd.DataFrame = pd.read_csv("coursework/docs/papers/cochran_default.csv")

row_means: list[float] = list()
dispersions: list[float] = list()

for index, row in df.iterrows():
    row_sum: float = 0
    values_count: int = len(row)
    sum_squared_diff: float = 0.0

    for value in row:
        row_sum += value

    row_means.append(row_sum / values_count)

    for value in row:
        sum_squared_diff += (value - row_means[-1]) ** 2

    dispersions.append(sum_squared_diff / (values_count - 1))

df['Mean'] = row_means
df['Dispersion'] = dispersions
df.to_csv("coursework/docs/papers/cochran_default.csv", index = False)

max_dispersion: float = df['Dispersion'].max()
sum_dispersion: float = df['Dispersion'].sum()
cochran_value: float = max_dispersion / sum_dispersion

print(f"dispersion: {df['Dispersion'].mean():.5f}")

threshold: float = 0.24

if cochran_value <= threshold:
    print("Cochran value is within acceptable limits.")
else:
    print("Cochran value exceeds acceptable limits.")

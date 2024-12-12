import os
import pandas as pd

directory = "coursework/docs/papers/"

aggregated_default_df = pd.DataFrame()

for filename in os.listdir(directory):
    if filename.startswith('cochran_') and filename[len('cochran_'):].split('.')[0].isdigit() and filename.endswith('.csv'):
        file_path = os.path.join(directory, filename)

        df = pd.read_csv(file_path)

        column_name = filename.split('_')[1].split('.')[0]
        aggregated_default_df[column_name] = df['Loss']

aggregated_default_df.to_csv("coursework/docs/papers/cochran_default.csv", index = False)

aggregated_verbose_df = pd.DataFrame()

for filename in os.listdir(directory):
    if filename.startswith('cochran_') and filename[len('cochran_'):].split('.')[0].isdigit() and filename.endswith('.csv'):
        file_path = os.path.join(directory, filename)

        df = pd.read_csv(file_path)
        column_name = filename.split('_')[1].split('.')[0]

        if aggregated_verbose_df.empty:
            aggregated_verbose_df = df.copy()
            aggregated_verbose_df.rename(columns = {"Loss": column_name}, inplace = True)
        else:
            aggregated_verbose_df[column_name] = df['Loss']

aggregated_verbose_df.to_csv("coursework/docs/papers/cochran_verbose.csv", index = False)

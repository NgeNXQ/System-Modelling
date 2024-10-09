import matplotlib.pyplot as plt

class VisualizerUtility:

    _output_path: str = "/out"

    @classmethod
    def setup(cls, output_path: str) -> None:
        cls._output_path = output_path

    @staticmethod
    def save_plot_histogram(numbers: list[float], title: str = "", data: dict[str, float] = {}) -> None:
        plt.figure(figsize = (12, 6))
        plt.hist(numbers, bins = 30, edgecolor = 'black')

        plt.title(title)
        plt.xlabel('Value')
        plt.ylabel('Frequency')

        annotation_text = "\n".join([f"{key}: {value:.5f}" for key, value in data.items()])
        plt.annotate(annotation_text, xy = (0.95, 0.95), xycoords = 'axes fraction',
                     fontsize = 12, verticalalignment = 'top', horizontalalignment = 'right')

        plt.savefig(f"{VisualizerUtility._output_path}{title}.png")
        plt.close()
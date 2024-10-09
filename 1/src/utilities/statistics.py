from scipy import stats
from common import IGenerator

class StatisticsUtility:

    @staticmethod
    def calculate_average(numbers: list[float]) -> float:
        return sum(numbers) / numbers.__len__()

    @staticmethod
    def calculate_variance(numbers: list[float]) -> float:
        average = StatisticsUtility.calculate_average(numbers)
        return sum(((number - average) ** 2) for number in numbers) / numbers.__len__()

    @staticmethod
    def evaluate_distribution(generator: IGenerator, samples: list[float], intervals_count: int, significance_level: float = 0.05) -> tuple[float, bool]:
        MIN_NUMBER = min(samples)
        MAX_NUMBER = max(samples)
        MIN_EXPECTED_FREQUENCY = 5
        LAST_INTERVAL_INDEX = intervals_count - 1

        intervals_sizes = [0 for _ in range(intervals_count + 1)]
        interval_width = (MAX_NUMBER - MIN_NUMBER) / intervals_count

        for number in samples:
            index = int((number - MIN_NUMBER) / interval_width)

            if number == MAX_NUMBER:
                index -= 1

            intervals_sizes[index] += 1

        chi2 = 0
        cumulative_observed = 0
        left_interval_index = 0
        right_interval_index = 0

        for i in range(intervals_count):
            cumulative_observed += intervals_sizes[i]

            if cumulative_observed < MIN_EXPECTED_FREQUENCY and i != LAST_INTERVAL_INDEX:
                continue

            right_interval_index = (i + 1)
            left_boundary = MIN_NUMBER + interval_width * left_interval_index
            right_boundary = MIN_NUMBER + interval_width * right_interval_index
            expected_count = samples.__len__() * (generator.calculate_distribution(right_boundary) - generator.calculate_distribution(left_boundary))

            chi2 += ((cumulative_observed - expected_count) ** 2) / expected_count

            cumulative_observed = 0
            left_interval_index = i + 1

        degrees_of_freedom = intervals_count - 1 - generator.get_parameters_count()
        chi_critical_value = stats.chi2.ppf(1.0 - significance_level, degrees_of_freedom)

        return (chi2, (chi2 < chi_critical_value))
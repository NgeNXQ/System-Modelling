from utilities import StatisticsUtility, VisualizerUtility
from generators import ExponentialGenerator, NormalGenerator, EvenGenerator

def run_even_generators(numbers_count: int, intervals_count: int) -> None:
    generator = EvenGenerator(5 ** 13, 2 ** 31)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Even n = {numbers_count}, a = 5^13, c = 2^31", stats_results)

    generator = EvenGenerator(9 ** 5, 6 ** 8)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Even n = {numbers_count}, a = 9^5, c = 6^8", stats_results)

    generator = EvenGenerator(5 ** 6, 3 ** 4)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Even n = {numbers_count}, a = 5^6, c = 3^4", stats_results)

def run_normal_generators(numbers_count: int, intervals_count: int) -> None:
    generator = NormalGenerator(-1, -2)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Normal n = {numbers_count}, alpha = -1, sigma = -2", stats_results)

    generator = NormalGenerator(5, 125)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Normal n = {numbers_count}, alpha = 5, sigma = 125", stats_results)

    generator = NormalGenerator(235, 2342)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Normal n = {numbers_count}, alpha = 235, sigma = 2342", stats_results)

def run_exponential_generators(numbers_count: int, intervals_count: int) -> None:
    generator = ExponentialGenerator(10)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Exponential n = {numbers_count}, lambda = 10", stats_results)

    generator = ExponentialGenerator(15)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Exponential n = {numbers_count}, lambda = 15", stats_results)

    generator = ExponentialGenerator(20)
    samples = generator.generate_samples(numbers_count)
    stats_results = {
        "Mean": StatisticsUtility.calculate_average(samples),
        "Variance": StatisticsUtility.calculate_variance(samples),
        "X^2 Value": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[0],
        "X^2 Validation": StatisticsUtility.evaluate_distribution(generator, samples, intervals_count)[1]
    }
    VisualizerUtility.save_plot_histogram(samples, f"Exponential n = {numbers_count}, lambda = 20", stats_results)

if __name__ == "__main__":

    VisualizerUtility.setup("../labs/1/docs/")

    intervals_count = 10
    numbers_count = 10_000

    run_even_generators(numbers_count, intervals_count)
    run_normal_generators(numbers_count, intervals_count)
    run_exponential_generators(numbers_count, intervals_count)
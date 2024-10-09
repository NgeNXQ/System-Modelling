import math
import random
from common import IGenerator

class NormalGenerator(IGenerator):

    def __init__(self, alpha: float, sigma: float) -> None:
        self._alpha = alpha
        self._sigma = sigma

    def get_parameters_count(self) -> int:
        return 2

    def generate_samples(self, numbers_count: int) -> list[float]:
        numbers: list[float] = []
        for _ in range(numbers_count):
            u = sum([random.random() for _ in range(1, 13)]) - 6
            numbers.append((self._sigma * u + self._alpha))
        return numbers

    def calculate_distribution(self, number: float) -> float:
        return (1 + math.erf((number - self._alpha) / (2 ** 0.5 * self._sigma))) / 2.0
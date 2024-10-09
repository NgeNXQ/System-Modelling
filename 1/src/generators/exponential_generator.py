import math
import random
from common import IGenerator

class ExponentialGenerator(IGenerator):

    def __init__(self, lambda_value: float) -> None:
        self._lambda_value = lambda_value

    def get_parameters_count(self) -> int:
        return 1

    def generate_samples(self, numbers_count: int) -> list[float]:
        numbers: list[float] = []
        for _ in range(numbers_count):
            numbers.append((-1 / self._lambda_value) * math.log(random.random()))
        return numbers

    def calculate_distribution(self, number: float) -> float:
        return 1 - math.exp(-self._lambda_value * number)
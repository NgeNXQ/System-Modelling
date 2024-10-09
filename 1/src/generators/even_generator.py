import math
import random
from common import IGenerator

class EvenGenerator(IGenerator):

    def __init__(self, a: float, c: float) -> None:
        self._a = a
        self._c = c
        self._z = random.random()

    def get_parameters_count(self) -> int:
        return 2

    def generate_samples(self, numbers_count: int) -> list[float]:
        numbers: list[float] = []
        for _ in range(numbers_count):
            self._z = math.fmod(self._a * self._z, self._c)
            numbers.append(self._z / self._c)
        return numbers

    def calculate_distribution(self, number: float) -> float:
        if number < 0:
            return 0
        elif number > 1:
            return 1
        else:
            return number
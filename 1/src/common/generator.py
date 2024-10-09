from abc import ABC, abstractmethod

class IGenerator(ABC):

    @abstractmethod
    def get_parameters_count(self) -> int:
        ...

    @abstractmethod
    def generate_samples(self, count: int) -> list[float]:
        ...

    @abstractmethod
    def calculate_distribution(self, number: float) -> float:
        ...
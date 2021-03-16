from abc import ABC, abstractmethod

class AbstractCommand(ABC):

    @abstractmethod
    def execute(self, **kwargs):
        pass

    @abstractmethod
    def get_purpose(self):
        pass
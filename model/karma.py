from datetime import datetime
from enum import Enum


class KarmaType(Enum):
    POZZYPOZ = 1
    NEGGYNEG = -1


class Karma(object):
    """docstring for Karma"""
    def __init__(self,
                 awarder="default awarder",
                 reason="default reason",
                 awarded=datetime.today(),
                 karma_type=KarmaType.POZZYPOZ):
        super(Karma, self).__init__()
        self.awarder = awarder
        self.reason = reason
        self.awarded = awarded
        self.karma_type = karma_type

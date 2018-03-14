from datetime import datetime
from pymodm import MongoModel, fields
from enum import Enum


class KarmaType(Enum):
    POZZYPOZ = 1
    NEGGYNEG = -1

class Karma(MongoModel):
    default_reason = "default reason"
    awarded_by_username = fields.CharField()
    reason = fields.CharField()
    awarded = fields.DateTimeField()
    karma_type = fields.CharField()

    # def __init__(self,
    #              awarded_by_username="default awarder",
    #              reason=default_reason,
    #              awarded=datetime.today(),
    #              karma_type=KarmaType.POZZYPOZ):
    #     super(Karma, self).__init__()
    #     self.awarded_by_username = awarded_by_username
    #     self.reason = reason
    #     self.awarded = awarded
    #     self.karma_type = karma_type
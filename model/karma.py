from datetime import datetime
from pymodm import MongoModel, fields
from enum import Enum

from pymongo import WriteConcern
from config import Config


class KarmaType(Enum):
    POZZYPOZ = 1
    NEGGYNEG = -1

class Karma(MongoModel):
    awarded_to_username = fields.CharField()
    default_reason = "default reason"
    awarded_by_username = fields.CharField()
    reason = fields.CharField()
    awarded = fields.DateTimeField()
    karma_type = fields.CharField()

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection

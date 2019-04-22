from pymodm import MongoModel, fields
from pymongo import WriteConcern
from enum import Enum

from config import Config

class FeatureRequestState(Enum):
    REQUESTED = 1
    INPROG = 2
    DELIVERED = 3

class FeatureRequest(MongoModel):
    requester = fields.CharField()
    request = fields.CharField()
    request_state = fields.CharField()

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
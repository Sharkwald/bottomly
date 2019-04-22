from pymodm import MongoModel, fields
from pymongo import WriteConcern
from enum import Enum

from config import Config


class FeatureRequestState(Enum):
    REQUESTED = 1
    IN_PROGRESS = 2
    DELIVERED = 3
    REJECTED = 4


class FeatureRequest(MongoModel):
    requester = fields.CharField()
    request = fields.CharField()
    request_state = fields.CharField()

    @staticmethod
    def _get_by_state(request_state: str) -> list:
        return list(FeatureRequest.objects.raw({'request_state': request_state}))

    @staticmethod
    def get_requested() -> list:
        return FeatureRequest._get_by_state(str(FeatureRequestState.REQUESTED))

    @staticmethod
    def get_in_progress() -> list:
        return FeatureRequest._get_by_state(str(FeatureRequestState.IN_PROGRESS))

    @staticmethod
    def get_delivered() -> list:
        return FeatureRequest._get_by_state(str(FeatureRequestState.DELIVERED))

    @staticmethod
    def get_rejected() -> list:
        return FeatureRequest._get_by_state(str(FeatureRequestState.REJECTED))

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection

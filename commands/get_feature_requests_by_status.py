from commands.abstract_command import AbstractCommand
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState


class GetFeatureRequestsByStatusCommand(AbstractCommand):

    def get_purpose(self):
        return "Gets the feature requests in the given request state"

    def execute(self, request_state) -> list:
        if request_state == FeatureRequestState.IN_PROGRESS:
            return FeatureRequest.get_in_progress()
        if request_state == FeatureRequestState.DELIVERED:
            return FeatureRequest.get_delivered()
        if request_state == FeatureRequestState.REJECTED:
            return FeatureRequest.get_rejected()
        return FeatureRequest.get_requested()

    def __init__(self):
        super(GetFeatureRequestsByStatusCommand, self)
        config = Config()
        config.connect_to_db()

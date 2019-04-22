# coding=utf-8
from commands.abstract_command import AbstractCommand
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState


class LogFeatureRequestCommand(AbstractCommand):

    def get_purpose(self):
        return "Logs a requested feature for this bot."

    def execute(self, requester, request):
        self.config.connect_to_db()
        fr = FeatureRequest(requester=requester,
                            request=request,
                            request_state=str(FeatureRequestState.REQUESTED))

        fr.save()
        return fr

    def __init__(self):
        super(LogFeatureRequestCommand, self)
        self.config = Config()

import os
import logging

from commands import GetFeatureRequestsByStatusCommand
from model.feature_request import FeatureRequestState
from slack_channel.abstract_event_handler import AbstractEventHandler

command_symbol = "requestedFeatures"
invalid_request_state_message = "That's not a valid request state, try asking for help."


def _build_response(request_state: FeatureRequestState, result: list) -> str:
    """List should be a collection of FeatureRequest objects"""
    response = f"Current feature requests with a state of {request_state.name.lower()}:" + os.linesep
    for r in result:
        response += f"\"{r.request}\" from {r.requester}"
        response += os.linesep
    return response.strip()


class GetFeatureRequestsEventHandler(AbstractEventHandler):
    @property
    def command(self) -> GetFeatureRequestsByStatusCommand:
        return GetFeatureRequestsByStatusCommand()

    @property
    def name(self):
        return "Get requested features by status"

    def _get_command_symbol(self):
        return command_symbol

    def get_usage(self):
        return self.command_trigger + "[requested|in_progress|delivered|rejected (default is requested)]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:-1])  # Trim the space off in case of no size arg

    def _invoke_handler_logic(self, slack_event):
        message = slack_event["text"]
        request_state = FeatureRequestState.REQUESTED
        request_state_arg = message[len(self.command_trigger):]
        try:
            if len(request_state_arg) > 0:
                request_state = FeatureRequestState[request_state_arg.upper()]
            result = self.command.execute(request_state)
            self._send_message_response(_build_response(request_state, result), slack_event)
        except KeyError:
            logging.exception("Error parsing supplied request state")
            self._send_message_response(invalid_request_state_message, slack_event)

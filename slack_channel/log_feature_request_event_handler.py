import logging

from commands import LogFeatureRequestCommand
from slack_channel.abstract_event_handler import AbstractEventHandler

command_symbol = "featureRequest"


class LogFeatureRequestEventHandler(AbstractEventHandler):
    @property
    def command(self) -> LogFeatureRequestCommand:
        return LogFeatureRequestCommand()

    @property
    def name(self) -> str:
        return "Log Feature Request"

    def get_usage(self):
        return self.command_trigger + "<request details>"

    def _get_command_symbol(self):
        return command_symbol

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        try:
            request = slack_event["text"][len(self.command_trigger):]
            requester = slack_event["user"]
            self.command.execute(request, requester)
            self._send_reaction_response(slack_event)
        except Exception as ex:
            logging.exception("Error logging feature request", ex)
            self._send_message_response("Unable to log this request. Please try again later.")

    def __init__(self, debug=False):
        self.debug = debug
        super(LogFeatureRequestEventHandler, self).__init__(debug)

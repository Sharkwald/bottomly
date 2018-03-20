# coding=utf-8
from commands import UrbanSearchCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "ud"
empty_result_message = "Left as an exercise for the reader."

class UrbanEventHandler(AbstractEventHandler):
    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        q = slack_event["text"][len(self.command_trigger):]
        c = UrbanSearchCommand()
        response_message = c.execute(q)
        if response_message is None:
            self._send_response(empty_result_message, slack_event)
        else :
            if self.debug:
                response_message = "[DEBUG] " + response_message
            self._send_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol

    def get_usage(self):
        return self.command_trigger + "<query>"
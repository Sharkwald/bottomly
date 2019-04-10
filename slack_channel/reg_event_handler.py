# coding=utf-8
from commands import RegSearchCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "vroom"
empty_result_message = "Left as an exercise for the reader."


class RegEventHandler(AbstractEventHandler):
    @property
    def command(self) -> RegSearchCommand:
        return RegSearchCommand()

    @property
    def name(self):
        return "AutoTrader Reg Lookup"

    def get_usage(self):
        return self.command_trigger + "<query>"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        q = slack_event["text"][len(self.command_trigger):]
        response_message = self.command.execute(q)
        if response_message is None:
            self._send_message_response(empty_result_message, slack_event)
        else:
            self._send_message_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol

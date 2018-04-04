# coding=utf-8
from commands import GiphyTranslateCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "gif"


class GiphyEventHandler(AbstractEventHandler):
    @property
    def command(self) -> GiphyTranslateCommand:
        return GiphyTranslateCommand()

    @property
    def name(self):
        return "Giphy"

    def get_usage(self):
        return self.command_trigger + "<query>"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        q = slack_event["text"][len(self.command_trigger):]
        result = self.command.execute(q)
        if result is None:
            response_message = "No gifs found for \"" + q + "\""
        else:
            response_message = result
        self._send_message_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol

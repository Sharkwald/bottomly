# coding=utf-8
from commands import GoogleSearchCommand
from slack_channel.abstract_event_handler import AbstractEventHandler

command_symbol = "g"


class GoogleEventHandler(AbstractEventHandler):

    @property
    def command(self):
        return GoogleSearchCommand()

    def get_usage(self):
        return self.command_trigger + "<query>"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def _invoke_handler_logic(self, slack_event):
        q = slack_event["text"][len(self.command_trigger):]
        c = self.command
        result = c.execute(q)
        if result is None:
            response_message = "No results found for \"" + q +  "\""
        else:
            response_message = result["title"] + " " + result["link"]
        self._send_message_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol

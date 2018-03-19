# coding=utf-8
from commands import GoogleSearchCommand
from slack_channel.abstract_event_handler import AbstractEventHandler

command_symbol = "g"


class GoogleEventHandler(AbstractEventHandler):
    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger)

    def handle(self, slack_event):
        q = slack_event["text"][len(self.command_trigger):]
        c = GoogleSearchCommand()
        result = c.execute(q)
        if result is None:
            response_message = f'No results found for "{q}"'
        else:
            response_message = "{title} {link}".format(**result)
        if self.debug:
            response_message = f"[DEBUG] {response_message}"
        self._send_response(response_message, slack_event)

    def _get_command_symbol(self):
        return command_symbol

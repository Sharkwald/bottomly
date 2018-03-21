import os

from commands import AddKarmaCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


class IncrementKarmaEventHandler(AbstractEventHandler):

    def _get_command_symbol(self):
        return "++"

    @property
    def command(self):
        return AddKarmaCommand()

    def get_usage(self):
        return self.command_trigger + "recipient [for <optional if recipient is a known username>] [reason]"

    def can_handle(self, slack_event):
        return False

    def _invoke_handler_logic(self, slack_event):
        command_text = slack_event['text'][2:]
        args = self.parse_command_text()


    def parse_command_text(self, command_text):
        command_args = command_text.split[" "]
        recipient = command_args[0]
        command_args.remove(recipient)
        command_args.remove("for")

        return {"recipient": recipient, "reason": ""}



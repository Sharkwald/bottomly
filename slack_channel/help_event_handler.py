# coding=utf-8
import os

from commands.abstract_command import AbstractCommand
from config import Config
from slack_channel.abstract_event_handler import AbstractEventHandler

command_symbols = ["help", "?", "list"]


class HelpEventHandler(AbstractEventHandler):
    @property
    def command(self) -> AbstractCommand:
        pass

    @property
    def name(self) -> str:
        return "Help"

    @property
    def _prefixed_command_symbols(self):
        prefix = self.config.get_prefix()
        return [prefix + c for c in command_symbols]

    def can_handle(self, slack_event) -> bool:
        split = slack_event["text"].split(" ")
        return split[0] in self._prefixed_command_symbols

    def _invoke_handler_logic(self, slack_event):
        help_text = ""
        for handler in self._command_handlers:
            help_text += handler.build_help_message() + os.linesep
        help_text = help_text.strip()
        self._send_dm_response(help_text, slack_event)

    def _get_command_symbol(self) -> str:
        return command_symbols[0]

    def get_usage(self) -> str:
        usage = ""
        for pc in self._prefixed_command_symbols:
            usage += "`" + pc + "` or "
        usage = usage[1:-5]
        return usage

    def _get_config(self):
        self.config = Config()
        self.command_trigger = self.config.get_prefix() + self._get_command_symbol() + " "

    def __init__(self, debug=False, command_handlers=list([])):
        super(HelpEventHandler, self).__init__(self)
        self._command_handlers = command_handlers
        self.debug = debug
        self._get_config()

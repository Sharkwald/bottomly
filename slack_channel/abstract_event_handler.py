import logging
import os
from abc import ABC, abstractmethod
from slacksocket import SlackSocket
from config import Config, ConfigKeys

help_token = "-?"

class AbstractEventHandler(ABC):

    @property
    @abstractmethod
    def command(self):
        pass

    @abstractmethod
    def can_handle(self, slack):
        pass

    @abstractmethod
    def _invoke_handler_logic(self, slack_event):
        pass

    @abstractmethod
    def _get_command_symbol(self):
        pass

    @abstractmethod
    def get_usage(self):
        pass

    def handle(self, slack_event):
        if slack_event["text"] == self.command_trigger + help_token:
            purpose = self.command.get_purpose() + os.linesep
            usage = "Usage: `" + self.get_usage() + "`"
            self._send_response(purpose + usage, slack_event)
        else:
            self._invoke_handler_logic(slack_event)


    def _send_response(self, response_message, slack_event):
        with SlackSocket(self.token) as s:
            msg = s.send_msg(response_message, slack_event["channel"])
            logging.info(msg.sent)

    def _get_config(self):
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.command_trigger = config.get_prefix() + self._get_command_symbol() + " "

    def __init__(self, debug=False):
        self.debug = debug
        self._get_config()
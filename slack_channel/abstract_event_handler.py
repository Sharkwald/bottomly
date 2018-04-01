import logging
import os
from abc import ABC, abstractmethod

from commands.abstract_command import AbstractCommand
from config import Config, ConfigKeys
from slack_channel.slack_message_broker import SlackMessageBroker

help_token = "-?"


class AbstractEventHandler(ABC):

    @property
    @abstractmethod
    def command(self) -> AbstractCommand:
        pass

    @property
    @abstractmethod
    def name(self) -> str:
        pass

    @abstractmethod
    def can_handle(self, slack_event) -> bool:
        pass

    @abstractmethod
    def _invoke_handler_logic(self, slack_event):
        pass

    @abstractmethod
    def _get_command_symbol(self) -> str:
        pass

    @abstractmethod
    def get_usage(self) -> str:
        pass

    @property
    def _help_message(self):
        return self.command_trigger + help_token

    def handle(self, slack_event):
        if self._is_help_event(slack_event):
            self._handle_help_event(slack_event)
        else:
            self._invoke_handler_logic(slack_event)

    def _is_help_event(self, slack_event):
        is_help_event = slack_event["text"] == self._help_message
        return is_help_event

    def _handle_help_event(self, slack_event):
        name = self.name + os.linesep
        purpose = self.command.get_purpose() + os.linesep
        usage = "Usage: `" + self.get_usage() + "`"
        message = name + purpose + usage
        self._send_message_response(message, slack_event)

    def _send_message_response(self, response_message, slack_event):
        self._slack_message_broker.send_message(response_message, slack_event["channel"])

    def _send_reaction_response(self, slack_event):
        self._slack_message_broker.send_reaction(slack_event)

    def _send_dm_response(self, response_message, slack_event):
        self._slack_message_broker.send_dm(response_message, slack_event["user_id"])

    def _get_config(self):
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.command_trigger = config.get_prefix() + self._get_command_symbol() + " "

    def __init__(self, debug=False):
        self._slack_message_broker = SlackMessageBroker(debug)
        self.debug = debug
        self._get_config()
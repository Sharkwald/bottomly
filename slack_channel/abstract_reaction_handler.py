import logging
import logging.config
import os
from os import path
from abc import ABC, abstractmethod

from commands.abstract_command import AbstractCommand
from config import Config, ConfigKeys
from slack_channel.slack_message_broker import SlackMessageBroker

log_file_path = path.join(path.dirname(path.abspath(__file__)), '../logging.conf')
logging.config.fileConfig(log_file_path)
logger = logging.getLogger('bottomly')

class AbstractReactionHandler(ABC):

    @property
    @abstractmethod
    def command(self) -> AbstractCommand:
        pass

    @abstractmethod
    def can_handle(self, slack_event) -> bool:
        pass

    @abstractmethod
    def _invoke_handler_logic(self, slack_event):
        pass

    def handle(self, reaction_add_event):
        logger.info(str(self) + " Handling: " + str(reaction_add_event))
        try:
            self._invoke_handler_logic(reaction_add_event)
        except Exception:
            logger.exception("Error thrown handling event: " + reaction_add_event)

    def _send_reaction_response(self, reaction_add_event):
        self._slack_message_broker.send_reaction(reaction_add_event["item"])

    def _get_config(self):
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)

    def __init__(self, debug=False):
        self._slack_message_broker = SlackMessageBroker(debug)
        self.debug = debug
        self._get_config()

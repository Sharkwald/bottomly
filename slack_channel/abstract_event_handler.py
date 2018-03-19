from abc import ABC, abstractmethod
from slacksocket import SlackSocket
from config import Config, ConfigKeys


class AbstractEventHandler(ABC):

    @abstractmethod
    def can_handle(self, slack):
        pass

    @abstractmethod
    def handle(self, slack_event):
        pass

    @abstractmethod
    def _get_command_symbol(self):
        pass

    def _send_response(self, response_message, slack_event):
        with SlackSocket(self.token) as s:
            msg = s.send_msg(response_message, slack_event["channel"])
            print(msg.sent)

    def _get_config(self):
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.command_trigger = config.get_prefix() + self._get_command_symbol() + " "

    def __init__(self, debug=False):
        self.debug = debug
        self._get_config()
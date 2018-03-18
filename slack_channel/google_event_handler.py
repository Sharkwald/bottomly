# coding=utf-8
from slacksocket import SlackSocket

from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys

command_symbol = "g"

class GoogleEventHandler(object):
    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger + " ")

    def handle(self, slack_event):
        q = slack_event["text"][3:]
        c = GoogleSearchCommand()
        result = c.execute(q)
        response_message = result["title"] + " " + result["link"]
        if self.debug:
            response_message = "[DEBUG] " + response_message
        self._send_response(response_message, slack_event)

    # TODO: Refactor these into a common base class
    def _send_response(self, response_message, slack_event):
        with SlackSocket(self.token) as s:
            msg = s.send_msg(response_message, slack_event["channel"])
            print(msg.sent)

    def _get_config(self):
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.command_trigger = config.get_prefix() + command_symbol

    def __init__(self, debug=False):
        self.debug = debug
        self._get_config()
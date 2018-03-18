# coding=utf-8
from slacksocket import SlackSocket

from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)

class GoogleEventHandler(object):
    def can_handle(self, slack_event):
        text = slack_event['text']
        return text.startswith("_g ")

    def handle(self, slack_event):
        q = slack_event['text'][3:]
        c = GoogleSearchCommand()
        result = c.execute(q)
        response_message = result['title'] + " " + result["link"]
        if self.debug:
            response_message = "[DEBUG] " + response_message
        self._send_google_response(response_message, slack_event)

    def _send_google_response(self, response_message, slack_event):
        with SlackSocket(token) as s:
            msg = s.send_msg(response_message, slack_event['channel'])
            print(msg.sent)

    def __init__(self, debug=False):
        self.debug = debug

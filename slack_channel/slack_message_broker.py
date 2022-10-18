# coding=utf-8
import logging
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError

from config import Config, ConfigKeys


class SlackMessageBroker(object):

    def send_reaction(self, slack_event_to_react_to):
        logging.info("Sending reaction")
        try:
            client = WebClient(token=self.token)
            client.reactions_add(name="robot_face",
                                 channel=slack_event_to_react_to["channel"],
                                 timestamp=slack_event_to_react_to["ts"])
        except SlackApiError:
            logging.exception("Error sending reaction to: " + str(slack_event_to_react_to))

    def send_message(self, message_text, channel, in_reply_to_ts=None):
        if message_text is None or message_text == "":
            return
        if self.debug:
            message_text = f"[{self.environment}] {message_text}"
        try:
            client = WebClient(token=self.token)
            msg_response = client.chat_postMessage(channel=channel, thread_ts=in_reply_to_ts, text=message_text)
            logging.info("Sent message: " + str(msg_response) + ", to channel: " + channel)
        except SlackApiError:
            logging.exception("Error sending message response to: " + channel)

    def send_dm(self, message_text, user_slack_id):
        if message_text is None or message_text == "":
            return
        try:
            client = WebClient(token=self.token)
            im = client.conversations_open(users=user_slack_id)
            channel_id = im["channel"]["id"]
            self.send_message(message_text, channel_id)
        except SlackApiError:
            logging.exception("Error sending DM response to: " + str(user_slack_id))

    def __init__(self, debug=False):
        self.debug = debug
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.environment = config.get_config_value(ConfigKeys.env_key)

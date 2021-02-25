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

    def send_message(self, message_text, channel):
        if message_text is None or message_text == "":
            return
        if self.debug:
            message_text = f"[{self.environment}] {message_text}"
        try:
            client = WebClient(token=self.token)
            msg_response = client.chat_postMessage(channel=channel, text=message_text)
            logging.info(msg_response)
        except SlackApiError:
            logging.exception("Error sending message response to: " + channel)

    def send_dm(self, message_text, user_slack_id):
        if message_text is None or message_text == "":
            return
        if self.debug:
            message_text = f"[{self.environment}] {message_text}"
        # try:
            # slack = Slacker(self.token)
            # im = slack.im.open(user_slack_id).body
            # if im["ok"]:
            #     channel_id = im["channel"]["id"]
            #     slack.chat.post_message(channel_id, text=message_text)
            #
            # else:
            #     raise Exception("Failed to open a DM to send response.")

        # except Exception:
        #     logging.exception("Error sending DM response to: " + str(user_slack_id))

    def __init__(self, debug=False):
        self.debug = debug
        config = Config()
        self.token = config.get_config_value(ConfigKeys.slack_bot_token)
        self.environment = config.get_config_value(ConfigKeys.env_key)

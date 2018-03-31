# coding=utf-8
import datetime
import logging

from slacker import Slacker
from slacksocket import SlackSocket
from config import Config, ConfigKeys
from model.member import Member
from slack_channel import *

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)


def _is_subscribed_event(slack_event):
    try:
        subscribed = True
        subscribed = subscribed and slack_event['type'] == "message"
        subscribed = subscribed and "text" in slack_event
        return subscribed
    except Exception as ex:
        logging.warning("Error determining if event is subscribed: " + str(ex))
        logging.warning("Message: " + slack_event)


class SlackEventHandler(object):
    def _init_command_handlers(self):
        self.command_handlers = list([
            GoogleEventHandler(self.debug),
            UrbanEventHandler(self.debug),
            WikipediaEventHandler(self.debug),
            IncrementKarmaEventHandler(self.debug),
            DecrementKarmaEventHandler(self.debug),
            GetCurrentNetKarmaEventHandler(self.debug),
            GetCurrentKarmaReasonsEventHandler(self.debug)
        ])

    def handle_slack_context(self):
        if self.debug:
            logging.info("Opening web socket to slack...")

        self._cache_channel_list()

        try:
            with SlackSocket(token) as s:
                self._process_slack_events(s)
        except Exception as ex:
            logging.exception("Error establishing connection to slack.")

    def _process_slack_events(self, slack):
        for e in slack.events():
            try:
                if self.debug:
                    logging.debug(e.json)

                slack_event = e.event
                if not _is_subscribed_event(slack_event):
                    continue

                self._insert_channel_id(slack_event)
                self._insert_user_id(slack_event)
                handled = self._execute_command_handlers(slack_event)

                # TODO: Dynamic response handler

            except Exception as ex:
                logging.exception("Error processing slack event", ex)

    def _execute_command_handlers(self, slack_event):
        handled = False
        for handler in self.command_handlers:
            if handler.can_handle(slack_event):
                handler.handle(slack_event)
                handled = True
                continue
        return handled

    def _insert_channel_id(self, slack_event):
        channel = list(filter((lambda c: c["name"] == slack_event["channel"]), self._channel_list))[0]
        slack_event["channel_id"] = channel["id"]

    def _insert_user_id(self, slack_event):
        member = Member.get_member_by_username(slack_event["user"])
        slack_event["user_id"] = member.slack_id

    def _cache_channel_list(self):
        slack = Slacker(token)
        self._channel_list = slack.channels.list().body["channels"]

    def __init__(self, debug=False):
        logging.basicConfig(filename="btmly_slack_connection_" + str(datetime.date.today()) + ".log",
                            level=logging.INFO)
        self.debug = debug
        self._init_command_handlers()
        super(SlackEventHandler, self).__init__()

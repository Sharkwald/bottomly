# coding=utf-8
import datetime
import logging
import logging.config

from slacker import Slacker
from slacksocket import SlackSocket
from config import Config, ConfigKeys
from model.member import Member
from slack_channel import *

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)

logging.config.fileConfig('logging.conf')
logger = logging.getLogger('bottomly')

def _is_subscribed_event(slack_event):
    try:
        subscribed = True
        subscribed = subscribed and slack_event['type'] == "message"
        subscribed = subscribed and "text" in slack_event
        subscribed = subscribed and "bot_id" not in slack_event
        return subscribed
    except Exception as ex:
        logger.warning("Error determining if event is subscribed: " + str(ex))
        logger.warning("Message: " + slack_event)
    return False


class SlackEventHandler(object):
    def _init_command_handlers(self):
        self._command_handlers = list([
            GoogleEventHandler(self.debug),
            UrbanEventHandler(self.debug),
            WikipediaEventHandler(self.debug),
            IncrementKarmaEventHandler(self.debug),
            DecrementKarmaEventHandler(self.debug),
            GetCurrentNetKarmaEventHandler(self.debug),
            GetCurrentKarmaReasonsEventHandler(self.debug),
            GiphyEventHandler(self.debug)
        ])

    def handle_slack_context(self):
        if self.debug:
            logger.info("Opening web socket to slack...")

        self._cache_channel_list()

        try:
            with SlackSocket(token) as s:
                self._process_slack_events(s)
        except Exception:
            logger.exception("Error establishing connection to slack.")

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

                handled = False

                help_handler = HelpEventHandler(self.debug, self._command_handlers)
                if help_handler.can_handle(slack_event):
                    handled = help_handler.handle(slack_event)

                if handled:
                    continue

                handled = self._execute_command_handlers(slack_event)

                if handled:
                    continue

                # TODO: Dynamic response handler

            except Exception:
                logger.exception("Error processing slack event")

    def _execute_command_handlers(self, slack_event):
        handled = False
        for handler in self._command_handlers:
            if handler.can_handle(slack_event):
                handler.handle(slack_event)
                handled = True
                continue
        return handled

    def _insert_channel_id(self, slack_event):
        try:
            known_channel_names = list(map((lambda c: c["name"]), self._channel_list))
            if slack_event["channel"] in known_channel_names:
                channel = list(filter((lambda c: c["name"] == slack_event["channel"]), self._channel_list))[0]
                slack_event["channel_id"] = channel["id"]
        except Exception:
            logger.exception("Error loading channel id from event: " + str(slack_event))

    def _insert_user_id(self, slack_event):
        try:
            member = Member.get_member_by_username(slack_event["user"])
            slack_event["user_id"] = member.slack_id
        except Exception:
            logger.exception("Error loading user id from event: " + str(slack_event))

    def _cache_channel_list(self):
        slack = Slacker(token)
        self._channel_list = slack.channels.list().body["channels"]

    def __init__(self, debug=False):
        self.debug = debug
        self._init_command_handlers()
        super(SlackEventHandler, self).__init__()

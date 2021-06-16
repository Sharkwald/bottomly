# coding=utf-8
import datetime
import logging
import logging.config

from slack_sdk import WebClient
from slack_sdk.rtm import RTMClient
from slack_sdk.errors import SlackApiError

from config import Config, ConfigKeys
from model.member import Member
from slack_channel import *

live_mode = "live"

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)
debug = Config().get_config_value(ConfigKeys.env_key) != live_mode

logging.config.fileConfig('logging.conf')
logger = logging.getLogger('bottomly')
for handler in logger.handlers:
    if debug:
        handler.setLevel(logging.INFO)
    else:
        handler.setLevel(logging.WARN)

_command_handlers = list([
    GoogleEventHandler(debug),
    UrbanEventHandler(debug),
    WikipediaEventHandler(debug),
    IncrementKarmaEventHandler(debug),
    DecrementKarmaEventHandler(debug),
    GetCurrentNetKarmaEventHandler(debug),
    GetCurrentKarmaReasonsEventHandler(debug),
    GiphyEventHandler(debug),
    GetLoserBoardEventHandler(debug),
    GetLeaderBoardEventHandler(debug),
    RegEventHandler(debug),
    TestEventHandler(debug),
    ReactionTestHandler(debug),
    GoogleImageEventHandler(debug)
])

_channel_list = WebClient(token=token).conversations_list().data.get("channels")


def _is_subscribed_event(slack_event):
    try:
        subscribed = True
        subscribed = subscribed and "text" in slack_event
        subscribed = subscribed and "bot_id" not in slack_event
        return subscribed
    except Exception as ex:
        logger.warning("Error determining if event is subscribed: " + str(ex))
        logger.warning("Message: " + str(slack_event))
    return False


def handle_slack_context():
    try:
        rtm_client = RTMClient(token=token)
        logging.info("Opening connection to slack.")
        rtm_client.start()
    except SlackApiError:
        logger.exception("Error establishing connection to slack.")


@RTMClient.run_on(event="message")
def _process_slack_event(**e):
    try:
        slack_event = e["data"]
        if not _is_subscribed_event(slack_event):
            return

        logging.info("Subscribing to: " + str(slack_event))

        _insert_channel_id(slack_event)
        _insert_username(slack_event)

        handled = False

        help_handler = HelpEventHandler(debug, _command_handlers)
        if help_handler.can_handle(slack_event):
            handled = help_handler.handle(slack_event)

        if handled:
            return

        handled = _execute_command_handlers(slack_event)

        if handled:
            return

        # TODO: Dynamic response handler

    except SlackApiError:
        logger.exception("Error processing slack event")


def _execute_command_handlers(slack_event):
    handled = False
    for handler in _command_handlers:
        if handler.can_handle(slack_event):
            handler.handle(slack_event)
            handled = True
            continue
    return handled


def _insert_channel_id(slack_event):
    try:
        known_channel_names = [c["name"] for c in _channel_list]
        if slack_event["channel"] in known_channel_names:
            channel = [c for c in _channel_list if c["name"] == slack_event["channel"]][0]
            slack_event["channel_id"] = channel["id"]
    except Exception:
        logger.exception("Error loading channel id from event: " + str(slack_event))


def _insert_username(slack_event):
    try:
        member = Member.get_member_by_slack_id(slack_event["user"])
        slack_event["user_id"] = slack_event["user"]
        slack_event["user"] = member.username

    except Exception:
        logger.exception("Error loading user id from event: " + str(slack_event))
